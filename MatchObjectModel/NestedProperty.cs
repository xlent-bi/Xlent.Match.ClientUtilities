using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{

    /// <summary>
    /// A property of properties.
    /// </summary>
    [DataContract]
    public class NestedProperty
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// A list of simple properties
        /// </summary>
        [DataMember]
        public List<Property> Properties { get; set; }
        /// <summary>
        /// A list of complex properties
        /// </summary>
        [DataMember]
        public List<NestedProperty> NestedProperties { get; set; }
    }
}
