using System;
using System.Collections.Generic;
using System.Linq;
using BibliotecaApp.Domain;
using BibliotecaApp.Infrastructure;

namespace BibliotecaApp.Services
{
    public class LibraryService
    {
        // Exponemos los repos concretos (coinciden con tus Forms)
        public InMemoryRepository<Book> Books { get; }
        public InMemoryRepository<User> Users { get; }
        public InMemoryRepository<Loan> Loans { get; }

        public LibraryService()
        {
            // 👇 Ajuste clave: pasar el getId requerido por tu InMemoryRepository<T>
            Books = new InMemoryRepository<Book>(b => b.Id);
            Users = new InMemoryRepository<User>(u => u.Id);
            Loans = new InMemoryRepository<Loan>(l => l.Id);
        }

        // --------------------------
        // CRUD Libros
        // --------------------------
        public void AddBook(Book b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (string.IsNullOrWhiteSpace(b.Category)) b.Category = "General";
            if (b.Copies < 0) b.Copies = 0;

            // No seteamos Id aquí (setter protegido). El modelo/repositorio se encarga.
            Books.Add(b);
        }

        public void UpdateBook(Book b)
        {
            if (b == null) throw new ArgumentNullException(nameof(b));
            Books.Update(b);
        }

        public void DeleteBook(Guid id) => Books.Delete(id);

        // --------------------------
        // CRUD Usuarios
        // --------------------------
        public void AddUser(User u)
        {
            if (u == null) throw new ArgumentNullException(nameof(u));
            if (string.IsNullOrWhiteSpace(u.Name)) throw new ArgumentException("El nombre es requerido");
            if (string.IsNullOrWhiteSpace(u.Email)) throw new ArgumentException("El email es requerido");

            Users.Add(u);
        }

        public void UpdateUser(User u)
        {
            if (u == null) throw new ArgumentNullException(nameof(u));
            Users.Update(u);
        }

        public void DeleteUser(Guid id) => Users.Delete(id);

        // --------------------------
        // Préstamos
        // --------------------------
        public void CreateLoan(Guid userId, Guid bookId)
        {
            var user = Users.GetById(userId);
            if (user == null || !user.IsActive) throw new InvalidOperationException("Usuario inválido o inactivo.");

            var book = Books.GetById(bookId);
            if (book == null) throw new InvalidOperationException("Libro no encontrado.");
            if (book.Copies <= 0) throw new InvalidOperationException("No hay copias disponibles.");

            // Descuenta copia
            book.Copies = Math.Max(0, book.Copies - 1);
            Books.Update(book);

            var loan = new Loan
            {
                UserId = userId,
                BookId = bookId,
                LoanDate = DateTime.Now,
                ReturnDate = null // IsReturned suele ser calculado a partir de esto
            };
            Loans.Add(loan);
        }

        public void ReturnLoan(Guid loanId)
        {
            var loan = Loans.GetById(loanId);
            if (loan == null) throw new InvalidOperationException("Préstamo no encontrado.");
            if (loan.ReturnDate != null) return; // ya devuelto

            loan.ReturnDate = DateTime.Now;
            Loans.Update(loan);

            var book = Books.GetById(loan.BookId);
            if (book != null)
            {
                book.Copies += 1;
                Books.Update(book);
            }
        }

        // --------------------------
        // Persistencia (3 JSON en una carpeta)
        // --------------------------
        public void SaveAll(string folder)
        {
            System.IO.Directory.CreateDirectory(folder);
            var usersPath = System.IO.Path.Combine(folder, "users.json");
            var booksPath = System.IO.Path.Combine(folder, "books.json");
            var loansPath = System.IO.Path.Combine(folder, "loans.json");

            // Estos métodos existen en el repo concreto InMemoryRepository<T>
            Users.SaveToFile(usersPath);
            Books.SaveToFile(booksPath);
            Loans.SaveToFile(loansPath);
        }

        public void LoadAll(string folder)
        {
            var usersPath = System.IO.Path.Combine(folder, "users.json");
            var booksPath = System.IO.Path.Combine(folder, "books.json");
            var loansPath = System.IO.Path.Combine(folder, "loans.json");

            if (System.IO.File.Exists(usersPath)) Users.LoadFromFile(usersPath);
            if (System.IO.File.Exists(booksPath)) Books.LoadFromFile(booksPath);
            if (System.IO.File.Exists(loansPath)) Loans.LoadFromFile(loansPath);
        }

        // --------------------------
        // Reportes (rúbrica: diccionarios)
        // --------------------------
        public Dictionary<Guid, int> CountLoansByBook(int year)
        {
            var dict = new Dictionary<Guid, int>();
            foreach (var l in Loans.GetAll().Where(x => x.LoanDate.Year == year))
            {
                if (!dict.ContainsKey(l.BookId)) dict[l.BookId] = 0;
                dict[l.BookId]++;
            }
            return dict;
        }

        public Dictionary<Guid, int> CountLoansByUser(int year)
        {
            var dict = new Dictionary<Guid, int>();
            foreach (var l in Loans.GetAll().Where(x => x.LoanDate.Year == year))
            {
                if (!dict.ContainsKey(l.UserId)) dict[l.UserId] = 0;
                dict[l.UserId]++;
            }
            return dict;
        }
    }
}
