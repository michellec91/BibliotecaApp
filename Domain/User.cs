using System;

namespace BibliotecaApp.Domain
{
    public class User : Person
    {
        private string _email = "";

        public string Email
        {
            get => _email;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
                    throw new ArgumentException("Email inválido.");
                _email = value.Trim();
            }
        }

        public bool IsActive { get; set; } = true;
    }
}
