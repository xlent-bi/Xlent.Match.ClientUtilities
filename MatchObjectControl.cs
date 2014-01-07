using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities
{
    public class MatchObjectControl
    {
        public MatchObject MatchObject { get; private set; }

        public Key MainKey { get { return MatchObject.Key; } }

        public MatchObjectControl(string clientName, string entity, string id, string matchId = null)
        {
            MatchObject = new MatchObject
            {
                Key = new Key
                {
                    ClientName = clientName,
                    EntityName = entity,
                    MatchId = matchId,
                    Value = id
                }
            };
        }

        public MatchObjectControl(MatchObject matchObject)
        {
            MatchObject = matchObject;
        }

        public MatchObjectControl(Key key, Data data)
        {
            MatchObject = new MatchObject
            {
                Key = key,
                Data = (data ?? new Data())
            };
        }


        public string GetPropertyValue(string name)
        {
            return GetPropertyValue(name, false);
        }

        public string GetPropertyValue(string name, bool okIfNotExist)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (MatchObject == null) throw new ArgumentNullException("this.MatchObject");
            if (MatchObject.Data == null)
            {
                if (okIfNotExist) return null;
                throw new ArgumentOutOfRangeException("name");
            }

            return MatchObject.Data.GetPropertyValue(name, okIfNotExist);
        }

        /// <summary>
        /// Update the current matchObject with the values in another matchObject.
        /// </summary>
        /// <param name="matchObject"></param>
        public void Update(MatchObject matchObject)
        {
            Debug.Assert(MatchObject != null);
            if (matchObject.Data == null)
            {
                this.MatchObject.Data = null;
            }
            else
            {
                this.MatchObject.Data = new Data();
                Update(matchObject.Data, this.MatchObject.Data);
            }
        }

        private void Update(Data source, Data target)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (source.Properties == null)
            {
                target.Properties = null;
            }
            else
            {
                target.Properties = new Dictionary<string, string>(source.Properties);
            }

            if (source.NestedProperties == null)
            {
                target.NestedProperties = null;
            }
            else
            {
                target.NestedProperties = new Dictionary<string, Data>(source.NestedProperties.Count);
                foreach (var nestedProperty in source.NestedProperties)
                {
                    var data = new Data();
                    Update(nestedProperty.Value, data);
                    target.NestedProperties.Add(nestedProperty.Key, data);
                }
            }

        }

        #region Test
        public void SetProperties(bool okIfNotExists, params string[] arguments)
        {
            if (arguments.Length < 1) return;

            if (null == MatchObject.Data)
            {
                MatchObject.Data = new Data();
            }

            if (null == MatchObject.Data.Properties)
            {
                MatchObject.Data.Properties = new Dictionary<string, string>(arguments.Length / 2);
            }

            for (int i = 0; i < arguments.Length; i += 2)
            {
                string name = arguments[i];
                string value = arguments[i + 1];
                MatchObject.Data.Properties[name] = value;
            }
        }


        public int FindDifferingProperty(params string[] arguments)
        {
            for (var i = 0; i < arguments.Length; i += 2)
            {
                var name = arguments[i];
                var value = arguments[i + 1];
                var oldValue = GetPropertyValue(name, true);
                if ((null == value) && (null == oldValue)) continue;
                if (value != oldValue)
                {
                    return i + 1;
                }
            }
            return 0;
        }
        #endregion
    }
}
