using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BibliotecaApp.Domain;
using BibliotecaApp.Services;

namespace BibliotecaApp
{
    public partial class UsersForm : Form
    {
        private readonly LibraryService _svc;

        private readonly ListBox lst = new ListBox();
        private readonly TextBox txtName = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly CheckBox chkActive = new CheckBox();
        private readonly ErrorProvider errorProvider = new ErrorProvider();

        private Guid? selectedId = null;
        private static readonly Regex EmailRegex =
            new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

        private class UserRow
        {
            public Guid Id { get; set; }
            public string Text { get; set; }
            public override string ToString() => Text;
        }

        public UsersForm(LibraryService svc)
        {
            InitializeComponent();   // Designer
            _svc = svc;

            // Limpia cualquier control de diseñador que tape
            Controls.Clear();
            SuspendLayout();

            Text = "Usuarios (CRUD)";
            Width = 900;
            Height = 600;

            errorProvider.ContainerControl = this;

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

            txtName.Width = 180; ApplyPlaceholder(txtName, "Nombre");
            txtEmail.Width = 220; ApplyPlaceholder(txtEmail, "Email");
            chkActive.Text = "Activo"; chkActive.Checked = true;

            var btnAdd = new Button { Text = "Agregar", Width = 90, Height = 28 };
            var btnUpdate = new Button { Text = "Actualizar", Width = 90, Height = 28 };
            var btnDelete = new Button { Text = "Eliminar", Width = 90, Height = 28 };
            var btnClear = new Button { Text = "Limpiar", Width = 90, Height = 28 };

            btnAdd.Click += (_, __) => AddUser();
            btnUpdate.Click += (_, __) => UpdateUser();
            btnDelete.Click += (_, __) => DeleteUser();
            btnClear.Click += (_, __) => ClearInputs();

            panel.Controls.AddRange(new Control[] { txtName, txtEmail, chkActive, btnAdd, btnUpdate, btnDelete, btnClear });
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
            tb.GotFocus += (s, e) => { if (tb.Text == placeholder) { tb.Text = ""; tb.ForeColor = SystemColors.WindowText; errorProvider.SetError(tb, ""); } };
            tb.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(tb.Text)) { tb.Text = placeholder; tb.ForeColor = Color.Gray; } };
        }
        private string GetInput(TextBox tb) { var ph = tb.Tag as string; return (ph != null && tb.Text == ph) ? "" : tb.Text; }

        private void RefreshList()
        {
            var data = _svc.Users.GetAll()
                .OrderBy(u => u.Name)
                .Select(u => new UserRow
                {
                    Id = u.Id,
                    Text = string.Format("{0} — {1} {2}", u.Name, u.Email, u.IsActive ? "(Activo)" : "(Inactivo)")
                })
                .ToList();

            lst.DataSource = null;
            lst.DataSource = data;

            selectedId = null;
            lst.ClearSelected();
        }

        private void BindSelection()
        {
            var row = lst.SelectedItem as UserRow;
            if (row == null) return;

            var u = _svc.Users.GetById(row.Id);
            if (u == null) return;

            selectedId = u.Id;
            txtName.Text = u.Name; txtName.ForeColor = SystemColors.WindowText;
            txtEmail.Text = u.Email; txtEmail.ForeColor = SystemColors.WindowText;
            chkActive.Checked = u.IsActive;
        }

        private bool ValidateInputs(out string message)
        {
            var name = GetInput(txtName);
            var email = GetInput(txtEmail);

            if (string.IsNullOrWhiteSpace(name)) { message = "El nombre es requerido."; errorProvider.SetError(txtName, message); return false; }
            if (string.IsNullOrWhiteSpace(email)) { message = "El email es requerido."; errorProvider.SetError(txtEmail, message); return false; }
            if (!EmailRegex.IsMatch(email)) { message = "Formato de email inválido."; errorProvider.SetError(txtEmail, message); return false; }

            var exists = _svc.Users.GetAll().Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                (!selectedId.HasValue || u.Id != selectedId.Value));
            if (exists) { message = "Ya existe un usuario con ese email."; errorProvider.SetError(txtEmail, message); return false; }

            errorProvider.SetError(txtName, ""); errorProvider.SetError(txtEmail, "");
            message = string.Empty; return true;
        }

        private void AddUser()
        {
            if (!ValidateInputs(out var msg)) { MessageBox.Show(msg, "Validación"); return; }
            try
            {
                var u = new User { Name = GetInput(txtName).Trim(), Email = GetInput(txtEmail).Trim(), IsActive = chkActive.Checked };
                _svc.AddUser(u);
                RefreshList(); ClearInputs();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        private void UpdateUser()
        {
            if (selectedId == null) { MessageBox.Show("Seleccione un usuario."); return; }
            if (!ValidateInputs(out var msg)) { MessageBox.Show(msg, "Validación"); return; }

            try
            {
                var u = _svc.Users.GetById(selectedId.Value);
                if (u == null) return;

                u.Name = GetInput(txtName).Trim();
                u.Email = GetInput(txtEmail).Trim();
                u.IsActive = chkActive.Checked;

                _svc.UpdateUser(u);
                RefreshList(); ClearInputs();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
        }

        private void DeleteUser()
        {
            if (selectedId == null) { MessageBox.Show("Seleccione un usuario."); return; }
            if (MessageBox.Show("¿Eliminar el usuario?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try { _svc.DeleteUser(selectedId.Value); RefreshList(); ClearInputs(); }
                catch (Exception ex) { MessageBox.Show(ex.Message, "Error"); }
            }
        }

        private void ClearInputs()
        {
            txtName.Text = (string)txtName.Tag; txtName.ForeColor = Color.Gray;
            txtEmail.Text = (string)txtEmail.Tag; txtEmail.ForeColor = Color.Gray;
            chkActive.Checked = true;
            selectedId = null;
            lst.ClearSelected();
            errorProvider.Clear();
        }
    }
}
