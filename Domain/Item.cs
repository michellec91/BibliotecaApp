using System;

namespace BibliotecaApp.Domain
{
    public abstract class Item : BaseEntity
    {
        private string _title = "";
        public string Title
        {
            get => _title;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El título es requerido.");
                _title = value.Trim();
            }
        }

        public string Category { get; set; } = "General";

        // Método polimórfico que cada tipo de ítem implementará a su manera
        public abstract string GetCatalogLine();
    }
}
