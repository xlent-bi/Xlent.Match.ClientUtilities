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

        /// <summary>
        /// Get the data for a certain object identity.
        /// </summary>
        /// <param name="key">The object identity.</param>
        /// <returns>Data representing the object.</returns>
        public ClientUtilities.MatchObjectModel.Data GetData(ClientUtilities.MatchObjectModel.Key key)
        {
            if (!_storage.ContainsKey(key)) throw new ArgumentOutOfRangeException("key");
            return _storage[key];
        }

        /// <summary>
        /// Update the internal representation of the object with received information.
        /// </summary>
        /// <param name="key">The object identity.</param>
        /// <param name="data">The new data for the object.</param>
        public void UpdateData(ClientUtilities.MatchObjectModel.Key key, ClientUtilities.MatchObjectModel.Data data)
        {
            if (!_storage.ContainsKey(key)) throw new ArgumentOutOfRangeException("key");
            _storage[key] = data;
        }

        /// <summary>
        /// Create a new internal object.
        /// </summary>
        /// <param name="key">The object identity.</param>
        /// <param name="data">The data for the object.</param>
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
