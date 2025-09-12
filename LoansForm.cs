using System;
using System.Linq;
using System.Windows.Forms;
using BibliotecaApp.Services;

namespace BibliotecaApp
{
    public partial class LoansForm : Form
    {
        private readonly LibraryService _svc;

        private readonly ComboBox cmbUsers = new ComboBox();
        private readonly ComboBox cmbBooks = new ComboBox();
        private readonly Button btnCreate = new Button();
        private readonly Button btnReturn = new Button();
        private readonly CheckBox chkSoloActivos = new CheckBox();
        private readonly ListBox lst = new ListBox();

        public class LoanRow
        {
            public Guid Id { get; set; }
            public string Text { get; set; }
            public override string ToString() => Text;
        }

        public LoansForm(LibraryService svc)
        {
            InitializeComponent();   // Designer
            _svc = svc;

            Controls.Clear();
            SuspendLayout();

            Text = "Préstamos";
            Width = 1000;
            Height = 650;

            BuildUi();

            ResumeLayout(true);

            LoadCombos();
            RefreshList();
        }

        private void BuildUi()
        {
            Controls.Clear();

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(8),
                AutoScroll = true
            };

            var lblUser = new Label { Text = "Usuario:", AutoSize = true, Margin = new Padding(0, 8, 4, 0) };
            var lblBook = new Label { Text = "Libro:", AutoSize = true, Margin = new Padding(8, 8, 4, 0) };

            cmbUsers.Width = 250; cmbBooks.Width = 420;
            cmbUsers.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBooks.DropDownStyle = ComboBoxStyle.DropDownList;

            btnCreate.Text = "Crear préstamo"; btnCreate.Width = 120; btnCreate.Height = 28;
            btnReturn.Text = "Devolver seleccionado"; btnReturn.Width = 160; btnReturn.Height = 28;

            chkSoloActivos.Text = "Solo activos"; chkSoloActivos.Checked = true;

            btnCreate.Click += (_, __) => CreateLoan();
            btnReturn.Click += (_, __) => ReturnSelected();
            chkSoloActivos.CheckedChanged += (_, __) => RefreshList();

            top.Controls.AddRange(new Control[] { lblUser, cmbUsers, lblBook, cmbBooks, btnCreate, btnReturn, chkSoloActivos });
            Controls.Add(top);

            lst.Dock = DockStyle.Fill;
            lst.Font = new System.Drawing.Font(System.Drawing.FontFamily.GenericSansSerif, 10);
            Controls.Add(lst);

            lst.BringToFront();
        }

        private void LoadCombos()
        {
            var users = _svc.Users.GetAll()
                .OrderBy(u => u.Name)
                .Select(u => new { u.Id, u.Name })
                .ToList();

            cmbUsers.DataSource = users;
            cmbUsers.DisplayMember = "Name";
            cmbUsers.ValueMember = "Id";

            var books = _svc.Books.GetAll()
                .OrderBy(b => b.Title)
                .Select(b => new { b.Id, Text = string.Format("{0} — {1} (Copias: {2})", b.Title, b.Author, b.Copies) })
                .ToList();

            cmbBooks.DataSource = books;
            cmbBooks.DisplayMember = "Text";
            cmbBooks.ValueMember = "Id";
        }

        private void RefreshList()
        {
            var loans = _svc.Loans.GetAll();
            if (chkSoloActivos.Checked) loans = loans.Where(l => !l.IsReturned);

            var rows = loans
                .OrderByDescending(l => l.LoanDate)
                .Select(l => new LoanRow
                {
                    Id = l.Id,
                    Text = string.Format("{0} — {1} | Prestado: {2}{3}",
                        _svc.Users.GetById(l.UserId) != null ? _svc.Users.GetById(l.UserId).Name : "(?)",
                        _svc.Books.GetById(l.BookId) != null ? _svc.Books.GetById(l.BookId).Title : "(?)",
                        l.LoanDate.ToString("yyyy-MM-dd HH:mm"),
                        l.IsReturned ? string.Format(" | Devuelto: {0}", l.ReturnDate.HasValue ? l.ReturnDate.Value.ToString("yyyy-MM-dd HH:mm") : "—") : "")
                })
                .ToList();

            lst.DataSource = null;
            lst.DataSource = rows;
        }

        private void CreateLoan()
        {
            try
            {
                if (cmbUsers.SelectedValue == null || cmbBooks.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione usuario y libro."); return;
                }

                var userId = (Guid)cmbUsers.SelectedValue;
                var bookId = (Guid)cmbBooks.SelectedValue;

                _svc.CreateLoan(userId, bookId);

                LoadCombos(); // actualiza copias
                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al crear préstamo");
            }
        }

        private void ReturnSelected()
        {
            var row = lst.SelectedItem as LoanRow;
            if (row == null) { MessageBox.Show("Seleccione un préstamo."); return; }

            try
            {
                _svc.ReturnLoan(row.Id);
                LoadCombos();
                RefreshList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error al devolver préstamo");
            }
        }
    }
}
