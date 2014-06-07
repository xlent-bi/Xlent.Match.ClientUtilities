﻿using System;
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
    [DataContract(Name = "Data", Namespace = "http://xlentmatch.com/")]
    public class Data
    {
        [DataMember]
        public Dictionary<string, string> Properties { get; set; }

        [DataMember]
        public Dictionary<string, Data> NestedProperties { get; set; }

        public Data()
        {
            Properties = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get a value from the <see cref="Properties"/> dictionary
        /// </summary>
        /// <param name="key">The name of the property.</param>
        /// <param name="okIfNotExists">True if it is OK if <paramref name="key"/> was not found in <see cref="Properties"/>.</param>
        /// <returns>The value found, or null if <paramref name="okIfNotExists"/> is true and <paramref name="key"/> was not found.</returns>
        public string GetPropertyValue(string key, bool okIfNotExists)
        {
            if ((Properties != null) && Properties.ContainsKey(key)) return Properties[key];
            if (okIfNotExists) return null;
            throw new ArgumentOutOfRangeException("key");
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

        public string FindDifferingPropertyName(params string[] arguments)
        {
            for (var i = 0; i < arguments.Length; i += 2)
            {
                var name = arguments[i];
                var value = arguments[i + 1];
                var oldValue = GetPropertyValue(name, true);
                if ((null == value) && (null == oldValue)) continue;
                if (value != oldValue)
                {
                    return name;
                }
            }
            return null;
        }

        public void Update(Data source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            Properties = source.Properties == null ? null : new Dictionary<string, string>(source.Properties);

            if (source.NestedProperties == null)
            {
                NestedProperties = null;
            }
            else
            {
                NestedProperties = new Dictionary<string, Data>(source.NestedProperties.Count);
                foreach (var nestedProperty in source.NestedProperties)
                {
                    var data = new Data();
                    data.Update(nestedProperty.Value);
                    NestedProperties.Add(nestedProperty.Key, data);
                }
            }
        }

        public void SetProperties(bool okIfNotExists, params string[] arguments)
        {
            if (arguments.Length < 1) return;

            for (int i = 0; i < arguments.Length; i += 2)
            {
                string name = arguments[i];
                string value = arguments[i + 1];
                SetPropertyValue(name, value);
            }
        }
    }
}
