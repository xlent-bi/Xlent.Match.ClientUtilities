﻿using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{
    /// <summary>
    ///     This class is an implementation of the MatchObject as specified at
    ///     http://www.xlentmatch.com/wiki/MatchObject
    /// </summary>
    [DataContract]
    public class MatchObject
    {
        public MatchObject(string clientName, string entityName, string keyValue, string matchId = null)
            : this(new Key(clientName, entityName, keyValue, matchId))
        {
        }

        public MatchObject(Key key, Data data = null)
        {
            Key = key;
            Data = data;
        }

        [DataMember]
        public Key Key { get; set; }

        [DataMember]
        public Data Data { get; set; }

        public string this[string name]
        {
            get { return GetPropertyValue(name); }
            set { SetProperty(name, value); }
        }

        public override bool Equals(object otherObject)
        {
            var o = otherObject as MatchObject;
            if (o == null) return false;

            return Key.Equals(o.Key);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override string ToString()
        {
            return Key.ToString();
        }

        public void SetProperties(bool okIfNotExists, params string[] arguments)
        {
            if (arguments.Length < 1) return;

            if (null == Data)
            {
                Data = new Data();
            }

            Data.SetProperties(okIfNotExists, arguments);
        }

        public void SetProperty(string path, string value)
        {
            if (null == Data)
            {
                Data = new Data();
            }

            Data.SetPropertyValue(path, value);
        }

        public string GetPropertyValue(string name)
        {
            if (null == Data) return null;

            return Data.GetPropertyValue(name, true);
        }
    }
}