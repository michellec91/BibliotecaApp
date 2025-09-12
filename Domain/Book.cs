using System;

namespace BibliotecaApp.Domain
{
    public class Book : Item
    {
        public string Author { get; set; } = "";

        private string _codigo = "";
        public string Codigo
        {
            get => _codigo;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El código es requerido.");
                _codigo = value.Trim();
            }
        }

        private int _copies = 1;
        public int Copies
        {
            get => _copies;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Copies), "No puede ser negativo.");
                _copies = value;
            }
        }

        public override string GetCatalogLine() => $"{Title} — {Author} [{Codigo}]";
    }
}