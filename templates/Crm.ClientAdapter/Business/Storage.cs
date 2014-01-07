using System;
using System.Collections.Generic;

using Crm.ClientAdapter.Model;

namespace Crm.ClientAdapter.Business
{
    public class Storage<T>
    {
        private Dictionary<int, T> _storage;
        private int _nextId = 0;

        public int NextId()
        {
            return _nextId++;
        }

        public Storage()
        {
            _storage = new Dictionary<int, T>();
        }

        public T GetData(int id)
        {
            if (!_storage.ContainsKey(id)) throw new ArgumentOutOfRangeException("id");
            return _storage[id];
        }

        public void SetData(int id, T data)
        {
            _storage[id] = data;
        }
    }
}
