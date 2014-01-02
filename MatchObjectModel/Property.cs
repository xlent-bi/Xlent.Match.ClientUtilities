using System;
using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{
    /// <summary>
    /// A simple property (name, type and value)
    /// </summary>
    [DataContract]
    public class Property
    {
        /// <summary>
        /// The Type of this property
        /// </summary>
        [DataMember]
        public string Type { get; set; }
        /// <summary>
        /// The name of this property
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// The value of the property
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// True if the property is of type Key
        /// </summary>
        public bool IsKey { get { return ("Key" == Type); } }

        /// <summary>
        /// True if the property is of type Reference
        /// </summary>
        public bool IsReference { get { return ("Reference" == Type); } }
        /// <summary>
        /// True for properties without a type
        /// </summary>
        public bool IsRegular { get { return String.IsNullOrEmpty(Type); } }

        public Property() { }

        public Property(string type, string name, string value)
        {
            if (!String.IsNullOrEmpty(type) && (type != "Key") && (type != "Reference"))
            {
                throw new ArgumentException("Expected value to be \"Key\", \"Reference\" or null.", "type");
            }

            this.Type = type;
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// The name 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public Property(string name, string value)
            : this((string)null, name, value)
        {
        }
    }
}
