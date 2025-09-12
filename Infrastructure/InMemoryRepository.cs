using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json; 

namespace BibliotecaApp.Infrastructure
{
    public class InMemoryRepository<T> : IRepository<T> where T : class
    {
        private readonly List<T> _items = new List<T>();
        private readonly Func<T, Guid> _getId;

        public InMemoryRepository(Func<T, Guid> getId)
        {
            _getId = getId;
        }

        public IEnumerable<T> GetAll()
        {
            return _items.ToList();
        }

        public T GetById(Guid id)
        {
            return _items.FirstOrDefault(x => _getId(x) == id);
        }

        public void Add(T entity)
        {
            _items.Add(entity);
        }

        public void Update(T entity)
        {
            var id = _getId(entity);
            var idx = _items.FindIndex(x => _getId(x) == id);
            if (idx >= 0)
                _items[idx] = entity;
        }

        public void Delete(Guid id)
        {
            var idx = _items.FindIndex(x => _getId(x) == id);
            if (idx >= 0)
                _items.RemoveAt(idx);
        }

      
        public void LoadFromFile(string path)
        {
            try
            {
                if (!File.Exists(path)) return;
                var json = File.ReadAllText(path);
                var list = JsonConvert.DeserializeObject<List<T>>(json);
                if (list == null) return;
                _items.Clear();
                _items.AddRange(list);
            }
            catch
            {
               
            }
        }

        public void SaveToFile(string path)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
                var json = JsonConvert.SerializeObject(_items, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch
            {
              
            }
        }
    }
}
