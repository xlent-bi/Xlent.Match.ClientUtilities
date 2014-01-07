using System;
using System.Collections.Generic;
using System.Configuration;
using Xlent.Match.ClientUtilities;
using Xlent.Match.Test.ClientAdapter;

namespace Xlent.Match.Test.ClientAdapter
{
    public class BusinessLogic
    {
        private Dictionary<ClientUtilities.MatchObjectModel.Key, ClientUtilities.MatchObjectModel.Data> _storage;
        private Dictionary<string, int> _nextId;

        private int NextId(string entityName)
        {
            if (null == _nextId)
            {
                _nextId = new Dictionary<string, int>();
            }
            if (!_nextId.ContainsKey(entityName))
            {
                _nextId[entityName] = 0;
            }

            return _nextId[entityName]++;
        }

        public BusinessLogic()
        {
            _storage = new Dictionary<ClientUtilities.MatchObjectModel.Key, ClientUtilities.MatchObjectModel.Data>();
        }

        public ClientUtilities.MatchObjectModel.Data GetData(ClientUtilities.MatchObjectModel.Key key)
        {
            if (!_storage.ContainsKey(key)) throw new ArgumentOutOfRangeException("key");
            return _storage[key];
        }

        public void UpdateData(ClientUtilities.MatchObjectModel.Key key, ClientUtilities.MatchObjectModel.Data data)
        {
            if (!_storage.ContainsKey(key)) throw new ArgumentOutOfRangeException("key");
            _storage[key] = data;
        }

        public string CreateData(ClientUtilities.MatchObjectModel.Key key, ClientUtilities.MatchObjectModel.Data data)
        {
            if (_storage.ContainsKey(key))
            {
                UpdateData(key, data);
                return key.Value;
            }
            key.Value = NextId(key.EntityName).ToString();
            _storage[key] = data;

            return key.Value;
        }
    }
}
