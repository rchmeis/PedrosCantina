using System;
using System.Collections.Generic;
using System.Text;

namespace PedrosCantinaLibrary.Repository
{
    public interface ICRUD<T>
    {
        public T Create(T item);
        public List<T> GetAll();
        public T GetById(string id);
        public T Update(string id, T item);
        public T Delete(string id);
    }
}
