using System;
using System.Windows.Forms;
using BibliotecaApp.Services;

namespace BibliotecaApp
{
    public partial class MainForm : Form
    {
        private readonly LibraryService _svc;
        private MenuStrip menu;

        public MainForm(LibraryService svc)
        {
            InitializeComponent();
            _svc = svc;

            Text = "Gestión de Biblioteca";
            Width = 1000;
            Height = 650;

            BuildUi();
                      
            this.FormClosing += (_, __) =>
            {
                try
                {
                    var dataDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
                    _svc.SaveAll(dataDir);
                }
                catch { /* ignorar errores de guardado al salir */ }
            };
        }

        private void BuildUi()
        {
            Controls.Clear();

           
            menu = new MenuStrip { Dock = DockStyle.Top };
            Controls.Add(menu);

            var mLibros = new ToolStripMenuItem("Libros");
            var mUsuarios = new ToolStripMenuItem("Usuarios");
            var mPrestamos = new ToolStripMenuItem("Préstamos");
            var mReportes = new ToolStripMenuItem("Reportes");

            mLibros.Click += (_, __) => new BooksForm(_svc).ShowDialog(this);
            mUsuarios.Click += (_, __) => new UsersForm(_svc).ShowDialog(this);
            mPrestamos.Click += (_, __) => new LoansForm(_svc).ShowDialog(this);
            mReportes.Click += (_, __) => new ReportsForm(_svc).ShowDialog(this);

            menu.Items.AddRange(new[] { mLibros, mUsuarios, mPrestamos, mReportes });
        }
    }
}