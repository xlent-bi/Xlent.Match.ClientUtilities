using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{
    /// <summary>
    /// Container of all data for the MatchObject.
    /// </summary>
    [DataContract]
    public class ObjectData
    {
        [DataMember]
        public List<Property> Properties { get; set; }
        [DataMember]
        public List<NestedProperty> NestedProperties { get; set; }
    }
}
