using System;

namespace BibliotecaApp.Domain
{
    public abstract class Person : BaseEntity
    {
        private string _name = "";

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("El nombre es requerido.");
                _name = value.Trim();
            }
        }

        public override string ToString() => Name;
    }
}
