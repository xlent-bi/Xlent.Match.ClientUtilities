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
        public MainKey MainKey { get; set; }
        [DataMember]
        public ObjectData ObjectData { get; set; }

        public static DataContractSerializer Serializer = new DataContractSerializer(typeof(MatchObject));

        public override bool Equals(object otherObject)
        {
            var o = otherObject as MatchObject;
            if (o == null) return false;

            return MainKey.Equals(o.MainKey);
        }

        public override int GetHashCode()
        {
            return MainKey.GetHashCode();
        }

        public override string ToString()
        {
            return MainKey.ToString();
        }
    }
}
