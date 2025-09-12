using System;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using BibliotecaApp.Services;

namespace BibliotecaApp
{
    public partial class ReportsForm : Form
    {
        private readonly LibraryService _svc;

        private readonly Chart chartBooks = new Chart();
        private readonly Chart chartUsers = new Chart();
        private readonly ListBox lstMatrix = new ListBox();
        private readonly ComboBox cmbYear = new ComboBox();
        private readonly Button btnRefresh = new Button();

        public ReportsForm(LibraryService svc)
        {
            InitializeComponent();
            _svc = svc;

            Controls.Clear();
            SuspendLayout();

            Text = "Reportes y Gráficos";
            Width = 1000;
            Height = 650;

            BuildUi();
            LoadYears();

            ResumeLayout(true);

            RefreshReports();
        }

        private void BuildUi()
        {
            Controls.Clear();

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 46,
                Padding = new Padding(8),
                AutoScroll = true
            };

            var lblYear = new Label { Text = "Año:", AutoSize = true, Margin = new Padding(0, 10, 6, 0) };
            cmbYear.Width = 100;

            btnRefresh.Text = "Actualizar";
            btnRefresh.Width = 100;
            btnRefresh.Click += (_, __) => RefreshReports();

            top.Controls.AddRange(new Control[] { lblYear, cmbYear, btnRefresh });
            Controls.Add(top);

            var mid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 360,
                ColumnCount = 2,
                RowCount = 1
            };
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            chartBooks.Dock = DockStyle.Fill;
            chartBooks.ChartAreas.Add(new ChartArea("ca1"));
            chartBooks.Titles.Add("Libros más prestados (Top 5)");
            chartBooks.Legends.Clear();

            chartUsers.Dock = DockStyle.Fill;
            chartUsers.ChartAreas.Add(new ChartArea("ca2"));
            chartUsers.Titles.Add("Usuarios más activos (Top 5)");
            chartUsers.Legends.Clear();

            mid.Controls.Add(chartBooks, 0, 0);
            mid.Controls.Add(chartUsers, 1, 0);
            Controls.Add(mid);

            var lblMatrix = new Label
            {
                Text = "Matriz Mes × Categoría (cantidad de préstamos)",
                Dock = DockStyle.Top,
                Padding = new Padding(8, 6, 0, 6)
            };
            Controls.Add(lblMatrix);

            lstMatrix.Dock = DockStyle.Fill;
            lstMatrix.Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 10);
            Controls.Add(lstMatrix);
        }

        private void LoadYears()
        {
            var years = _svc.Loans.GetAll()
                .Select(l => l.LoanDate.Year)
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            if (years.Count == 0) years.Add(DateTime.Now.Year);

            cmbYear.DataSource = years;
            cmbYear.SelectedItem = years[years.Count - 1];
        }

        private void RefreshReports()
        {
            int year = (cmbYear.SelectedItem is int) ? (int)cmbYear.SelectedItem : DateTime.Now.Year;

            // ---------- Top Libros (usa Dictionary<Guid,int>) ----------
            var byBook = _svc.CountLoansByBook(year);
            var topBooks = byBook
                .Select(kvp => new
                {
                    BookId = kvp.Key,
                    Count = kvp.Value,
                    Title = _svc.Books.GetById(kvp.Key) != null ? _svc.Books.GetById(kvp.Key).Title : "(desconocido)"
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            chartBooks.Series.Clear();
            var s1 = new Series("Libros") { ChartType = SeriesChartType.Column, ChartArea = "ca1" };
            foreach (var x in topBooks)
                s1.Points.AddXY(x.Title, x.Count);
            chartBooks.Series.Add(s1);

            // ---------- Top Usuarios (usa Dictionary<Guid,int>) ----------
            var byUser = _svc.CountLoansByUser(year);
            var topUsers = byUser
                .Select(kvp => new
                {
                    UserId = kvp.Key,
                    Count = kvp.Value,
                    Name = _svc.Users.GetById(kvp.Key) != null ? _svc.Users.GetById(kvp.Key).Name : "(desconocido)"
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            chartUsers.Series.Clear();
            var s2 = new Series("Usuarios") { ChartType = SeriesChartType.Column, ChartArea = "ca2" };
            foreach (var x in topUsers)
                s2.Points.AddXY(x.Name, x.Count);
            chartUsers.Series.Add(s2);

            // ---------- Matriz Mes × Categoría (usa int[,] y string[] ) ----------
            var categories = _svc.Books.GetAll()
                .Select(b => string.IsNullOrWhiteSpace(b.Category) ? "General" : b.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToArray(); // arreglo

            int[,] matrix = BuildMonthCategoryMatrix(year, categories);

            lstMatrix.Items.Clear();
            if (categories.Length == 0)
            {
                lstMatrix.Items.Add("No hay categorías registradas.");
                return;
            }

            lstMatrix.Items.Add("Matriz Mes × Categoría (cantidad de préstamos)");
            lstMatrix.Items.Add("------------------------------------------------");

            for (int month = 1; month <= 12; month++)
            {
                for (int ci = 0; ci < categories.Length; ci++)
                {
                    int count = matrix[month - 1, ci];
                    if (count > 0)
                        lstMatrix.Items.Add(string.Format("{0} — {1}: {2}", MonthName(month), categories[ci], count));
                }
            }

            if (lstMatrix.Items.Count <= 2)
                lstMatrix.Items.Add("Sin datos para el año seleccionado.");
        }

        private int[,] BuildMonthCategoryMatrix(int year, string[] categories)
        {
            int months = 12;
            int cats = categories.Length;
            int[,] mat = new int[months, cats]; // MATRIZ

            var catIndex = new System.Collections.Generic.Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < cats; i++) catIndex[categories[i]] = i;

            var loansYear = _svc.Loans.GetAll().Where(l => l.LoanDate.Year == year).ToList();

            foreach (var l in loansYear)
            {
                var b = _svc.Books.GetById(l.BookId);
                var cat = (b != null && !string.IsNullOrWhiteSpace(b.Category)) ? b.Category : "General";
                int monthIndex = l.LoanDate.Month - 1;
                int catIdx;
                if (!catIndex.TryGetValue(cat, out catIdx)) continue;

                mat[monthIndex, catIdx] += 1;
            }

            return mat;
        }

        private static string MonthName(int m)
        {
            switch (m)
            {
                case 1: return "Enero";
                case 2: return "Febrero";
                case 3: return "Marzo";
                case 4: return "Abril";
                case 5: return "Mayo";
                case 6: return "Junio";
                case 7: return "Julio";
                case 8: return "Agosto";
                case 9: return "Septiembre";
                case 10: return "Octubre";
                case 11: return "Noviembre";
                case 12: return "Diciembre";
                default: return m.ToString();
            }
        }
    }
}
