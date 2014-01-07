using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{
    /// <summary>
    /// This class is an implementation of the MatchObject as specified at
    /// http://www.xlentmatch.com/wiki/MatchObject
    /// </summary>
    [DataContract]
    public class MatchObject
    {
        [DataMember]
        public Key Key { get; set; }
        [DataMember]
        public Data Data { get; set; }

        public static DataContractSerializer Serializer = new DataContractSerializer(typeof(MatchObject));

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
    }
}
