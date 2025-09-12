using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BibliotecaApp.Domain;
using BibliotecaApp.Services;

namespace BibliotecaApp
{
    public partial class BooksForm : Form
    {
        private readonly LibraryService _svc;

        private readonly ListBox lst = new ListBox();
        private readonly TextBox txtTitle = new TextBox();
        private readonly TextBox txtAuthor = new TextBox();
        private readonly TextBox txtCodigo = new TextBox();
        private readonly TextBox txtCategory = new TextBox();
        private readonly NumericUpDown numCopies = new NumericUpDown();

        private Guid? selectedId = null;

        private class BookRow
        {
            public Guid Id { get; set; }
            public string Text { get; set; }
            public override string ToString() => Text;
        }

        public BooksForm(LibraryService svc)
        {
            InitializeComponent();  
            _svc = svc;

            Controls.Clear();
            SuspendLayout();

            Text = "Libros (CRUD)";
            Width = 900;
            Height = 600;

            BuildUi();

            ResumeLayout(true);
            RefreshList();
        }

        private void BuildUi()
        {
            Controls.Clear();

            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(8),
                AutoScroll = true
            };

            txtTitle.Width = 160; ApplyPlaceholder(txtTitle, "Título");
            txtAuthor.Width = 160; ApplyPlaceholder(txtAuthor, "Autor");
            txtCodigo.Width = 120; ApplyPlaceholder(txtCodigo, "Código");
            txtCategory.Width = 120; ApplyPlaceholder(txtCategory, "Categoría");

            numCopies.Minimum = 0; numCopies.Maximum = 100; numCopies.Value = 1; numCopies.Width = 80;

            var btnAdd = new Button { Text = "Agregar", Width = 90, Height = 28 };
            var btnUpdate = new Button { Text = "Actualizar", Width = 90, Height = 28 };
            var btnDelete = new Button { Text = "Eliminar", Width = 90, Height = 28 };
            var btnClear = new Button { Text = "Limpiar", Width = 90, Height = 28 };

            btnAdd.Click += (_, __) => AddBook();
            btnUpdate.Click += (_, __) => UpdateBook();
            btnDelete.Click += (_, __) => DeleteBook();
            btnClear.Click += (_, __) => ClearInputs();

            panel.Controls.AddRange(new Control[] { txtTitle, txtAuthor, txtCodigo, txtCategory, numCopies, btnAdd, btnUpdate, btnDelete, btnClear });
            Controls.Add(panel);

            lst.Dock = DockStyle.Fill;
            lst.Font = new Font(FontFamily.GenericSansSerif, 10);
            lst.SelectedIndexChanged += (_, __) => BindSelection();
            Controls.Add(lst);

            lst.BringToFront();
        }

        private void ApplyPlaceholder(TextBox tb, string placeholder)
        {
            tb.Tag = placeholder; tb.Text = placeholder; tb.ForeColor = Color.Gray;
            tb.GotFocus += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = "";
                    tb.ForeColor = SystemColors.WindowText;
                }
            };
            tb.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
                }
            };
        }

        private string GetInput(TextBox tb)
        {
            var ph = tb.Tag as string;
            return (ph != null && tb.Text == ph) ? "" : tb.Text;
        }

        private void RefreshList()
        {
            var data = _svc.Books.GetAll()
                .OrderBy(b => b.Title)
                .Select(b => new BookRow
                {
                    Id = b.Id,
                    Text = $"{b.Title} — {b.Author} | Código: {b.Codigo} | Categoría: {b.Category} | Copias: {b.Copies}"
                })
                .ToList();

            lst.DataSource = null;
            lst.DataSource = data;

            selectedId = null;
            lst.ClearSelected();
        }

        private void BindSelection()
        {
            var row = lst.SelectedItem as BookRow;
            if (row == null) return;

            var b = _svc.Books.GetById(row.Id);
            if (b == null) return;

            selectedId = b.Id;

            txtTitle.Text = b.Title; txtTitle.ForeColor = SystemColors.WindowText;
            txtAuthor.Text = b.Author; txtAuthor.ForeColor = SystemColors.WindowText;
            txtCodigo.Text = b.Codigo; txtCodigo.ForeColor = SystemColors.WindowText;
            txtCategory.Text = b.Category; txtCategory.ForeColor = SystemColors.WindowText;
            numCopies.Value = b.Copies;
        }

        private void AddBook()
        {
            try
            {
                var b = new Book
                {
                    Title = GetInput(txtTitle).Trim(),
                    Author = GetInput(txtAuthor).Trim(),
                    Codigo = GetInput(txtCodigo).Trim(),
                    Category = GetInput(txtCategory).Trim(),
                    Copies = (int)numCopies.Value
                };

                _svc.AddBook(b);
                RefreshList();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void UpdateBook()
        {
            if (selectedId == null) { MessageBox.Show("Seleccione un libro."); return; }

            try
            {
                var b = _svc.Books.GetById(selectedId.Value);
                if (b == null) return;

                b.Title = GetInput(txtTitle).Trim();
                b.Author = GetInput(txtAuthor).Trim();
                b.Codigo = GetInput(txtCodigo).Trim();
                b.Category = GetInput(txtCategory).Trim();
                b.Copies = (int)numCopies.Value;

                _svc.UpdateBook(b);
                RefreshList();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void DeleteBook()
        {
            if (selectedId == null) { MessageBox.Show("Seleccione un libro."); return; }

            if (MessageBox.Show("¿Eliminar el libro?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    _svc.DeleteBook(selectedId.Value);
                    RefreshList();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }
        }

        private void ClearInputs()
        {
            txtTitle.Text = (string)txtTitle.Tag; txtTitle.ForeColor = Color.Gray;
            txtAuthor.Text = (string)txtAuthor.Tag; txtAuthor.ForeColor = Color.Gray;
            txtCodigo.Text = (string)txtCodigo.Tag; txtCodigo.ForeColor = Color.Gray;
            txtCategory.Text = (string)txtCategory.Tag; txtCategory.ForeColor = Color.Gray;
            numCopies.Value = 1;
            selectedId = null;
            lst.ClearSelected();
        }
    }
}