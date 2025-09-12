using System;
using System.Collections.Generic;

namespace BibliotecaApp.Infrastructure
{
   
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(Guid id);  
        void Add(T entity);
        void Update(T entity);
        void Delete(Guid id);
    }
}
