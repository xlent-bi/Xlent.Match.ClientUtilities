using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{
    /// <summary>
    /// Container for all data for a <see cref=" MatchObject"/>.
    /// </summary>
    [DataContract]
    public class Data
    {
        [DataMember]
        public Dictionary<string, string> Properties { get; set; }

        [DataMember]
        public Dictionary<string, Data> NestedProperties { get; set; }

        /// <summary>
        /// Get a value from the <see cref="Properties"/> dictionary
        /// </summary>
        /// <param name="key">The name of the property.</param>
        /// <param name="okIfNotExists">True if it is OK if <paramref name="key"/> was not found in <see cref="Properties"/>.</param>
        /// <returns>The value found, or null if <paramref name="okIfNotExists"/> is true and <paramref name="key"/> was not found.</returns>
        public string GetPropertyValue(string key, bool okIfNotExists)
        {
            if (!Properties.ContainsKey(key))
            {
                if (okIfNotExists) return null;
                throw new ArgumentOutOfRangeException("key");
            }

            return Properties[key];
        }

        /// <summary>
        /// Get a nested property from the <see cref="NestedProperties"/> dictionary
        /// </summary>
        /// <param name="key">The name of the nested property.</param>
        /// <param name="okIfNotExists">True if it is OK if <paramref name="key"/> was not found in <see cref="NestedProperties"/>.</param>
        /// <returns>The nested property found, or null if <paramref name="okIfNotExists"/> is true and <paramref name="key"/> was not found.</returns>
        public Data GetNestedProperty(string key, bool okIfNotExists)
        {
            if (!NestedProperties.ContainsKey(key))
            {
                if (okIfNotExists) return null;
                throw new ArgumentOutOfRangeException("key");
            }

            return NestedProperties[key];
        }

        /// <summary>
        /// Find a possibly deeply nested property.
        /// </summary>
        /// <param name="path">The path to the property expressed as NestedPropertyName.Name, e.g. "Mother.Address.ZipCode".</param>
        /// <param name="okIfNotExists">True if it is OK if <paramref name="path"/> was not found.</param>
        /// <returns>The found value, or null if <paramref name="okIfNotExists"/> is true and the <paramref name="path"/> property was not found.</returns>
        public string FindPropertyValue(string path, bool okIfNotExists)
        {
            var pathArray = path.Split('.');
            return FindPropertyValue(okIfNotExists, new Queue<string>(pathArray));
        }

        private string FindPropertyValue(bool okIfNotExists, Queue<string> path)
        {
            var key = path.Dequeue();
            if (path.Count == 0)
            {
                return GetPropertyValue(key, okIfNotExists);
            }

            var data = GetNestedProperty(key, okIfNotExists);
            if (data == null) return null;

            return data.FindPropertyValue(okIfNotExists, path);
        }

        /// <summary>
        /// Find a possibly deeply nested property.
        /// </summary>
        /// <param name="path">The path to the property expressed as NestedPropertyName.Name, e.g. "Mother.Address.ZipCode".</param>
        /// <param name="okIfNotExists">True if it is OK if <paramref name="path"/> was not found.</param>
        /// <returns>The found value, or null if <paramref name="okIfNotExists"/> is true and the <paramref name="path"/> property was not found.</returns>
        public void SetPropertyValue(string path, string value)
        {
            var pathArray = path.Split('.');
            SetPropertyValue(value, new Queue<string>(pathArray));
        }

        private bool SetPropertyValue(string value, Queue<string> path)
        {
            var key = path.Dequeue();
            if (path.Count == 0)
            {
                if (Properties == null) Properties = new Dictionary<string, string>();
                Properties[key] = value;
                if (value == null)
                {
                    // Remove the value
                    Properties.Remove(key);
                    if (Properties.Count == 0)
                    {
                        Properties = null;
                        return true;
                    }
                }
                return false;
            }

            var data = GetNestedProperty(key, true);
            if (data == null)
            {
                data = new Data();
                NestedProperties[key] = data;
            }

            var remove = SetPropertyValue(value, path);
            if (remove)
            {
                NestedProperties.Remove(key);
                if (NestedProperties.Count == 0)
                {
                    NestedProperties = null;
                    return true;
                }
            }
            return false;
        }
    }
}
