using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{
    /// <summary>
    ///     Container for all data for a <see cref=" MatchObject" />.
    /// </summary>
    [DataContract(Name = "Data", Namespace = "http://xlentmatch.com/")]
    public class Data
    {
        public Data()
        {
            Properties = new Dictionary<string, string>();
        }

        [DataMember]
        public Dictionary<string, string> Properties { get; set; }

        [DataMember]
        public Dictionary<string, Data> NestedProperties { get; set; }

        /// <summary>
        ///     Get a value from the <see cref="Properties" /> dictionary
        /// </summary>
        /// <param name="key">The name of the property.</param>
        /// <param name="okIfNotExists">True if it is OK if <paramref name="key" /> was not found in <see cref="Properties" />.</param>
        /// <returns>
        ///     The value found, or null if <paramref name="okIfNotExists" /> is true and <paramref name="key" /> was not
        ///     found.
        /// </returns>
        public string GetPropertyValue(string key, bool okIfNotExists)
        {
            if ((Properties != null) && Properties.ContainsKey(key)) return Properties[key];
            if (okIfNotExists) return null;
            throw new ArgumentOutOfRangeException("key");
        }

        /// <summary>
        ///     Get a nested property from the <see cref="NestedProperties" /> dictionary
        /// </summary>
        /// <param name="key">The name of the nested property.</param>
        /// <param name="okIfNotExists">
        ///     True if it is OK if <paramref name="key" /> was not found in
        ///     <see cref="NestedProperties" />.
        /// </param>
        /// <returns>
        ///     The nested property found, or null if <paramref name="okIfNotExists" /> is true and <paramref name="key" />
        ///     was not found.
        /// </returns>
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
        ///     Find a possibly deeply nested property.
        /// </summary>
        /// <param name="path">The path to the property expressed as NestedPropertyName.Name, e.g. "Mother.Address.ZipCode".</param>
        /// <param name="okIfNotExists">True if it is OK if <paramref name="path" /> was not found.</param>
        /// <returns>
        ///     The found value, or null if <paramref name="okIfNotExists" /> is true and the <paramref name="path" />
        ///     property was not found.
        /// </returns>
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

        public string FindDifferingPropertyName(params string[] propertyNamesAndValues)
        {
            string propertyName;
            string expectedValue;
            FindDifferingPropertyName(out propertyName, out expectedValue, ConvertToDictionary(propertyNamesAndValues));
            return propertyName;
        }

        private static Dictionary<string, string> ConvertToDictionary(string[] propertyNamesAndValues)
        {
            var dictionary = new Dictionary<string, string>();

            for (var i = 0; i < propertyNamesAndValues.Length; i +=2)
            {
                dictionary.Add(propertyNamesAndValues[i], propertyNamesAndValues[i+1]);
            }
            return dictionary;
        }


        public void FindDifferingPropertyName(out string propertyName, out string expectedValue,
            IReadOnlyDictionary<string, string> propertyNamesAndValues)
        {
            foreach (var propertyNameAndValue in propertyNamesAndValues)
            {
                 propertyName = propertyNameAndValue.Key;
                expectedValue = propertyNameAndValue.Value;
                var value = GetPropertyValue(propertyName, true);
                if ((null == expectedValue) && (null == value)) continue;
                if (value != expectedValue)
                {
                    return;
                }
            }
            propertyName = null;
            expectedValue = null;
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

            for (var i = 0; i < arguments.Length; i += 2)
            {
                var name = arguments[i];
                var value = arguments[i + 1];
                SetPropertyValue(name, value);
            }
        }

        public override string ToString()
        {
            if ((Properties == null) || (Properties.Count == 0))
            {
                return "No properties";
            }
            return String.Join(", ", Properties.Select(pair => String.Format("{0}=\"{1}\"", pair.Key, pair.Value)));
        }

        public void SetProperties(bool okIfNotExists, IReadOnlyDictionary<string, string> propertyNamesAndValues)
        {
            if (propertyNamesAndValues == null) throw new ArgumentNullException("propertyNamesAndValues");
            if (propertyNamesAndValues.Count < 1) return;

            foreach (var pv in propertyNamesAndValues)
            {
                SetPropertyValue(pv.Key, pv.Value);
            }
        }
    }
}