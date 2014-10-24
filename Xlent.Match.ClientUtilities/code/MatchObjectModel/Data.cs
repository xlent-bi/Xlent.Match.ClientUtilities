using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{
    /// <summary>
    ///     Container for all data for a <see cref=" MatchObject" />.
    /// </summary>
    [DataContract(Name = "Data", Namespace = "http://xlentmatch.com/")]
    public class Data
    {
        private static string CheckSumToBeIgnored = "_IGNORE_";

        public Data()
        {
            Properties = new CaseInsensitiveDictionary<string>();
        }

        [DataMember]
        public CaseInsensitiveDictionary<string> Properties { get; set; }

        [DataMember]
        public CaseInsensitiveDictionary<Data> NestedProperties { get; set; }

        /// <summary>
        ///     A checksum for the data.
        ///     When the client sends data to Match, it sets this by calling <see cref="CalculateCheckSum" />.
        ///     When Match sends data to the client, it sets this value to the last checksum it received from the client for this
        ///     key.
        ///     That way, the client can verify that the updated data from Match was based on the current data in the client.
        /// </summary>
        [DataMember]
        public string CheckSum { get; private set; }

        /// <summary>
        ///     Calculate a check sum for a key value and some properties.
        /// </summary>
        /// <param name="keyValue">This string will be included in the check sum</param>
        /// <param name="properties">The properties that we will calculate the checksum for.</param>
        /// <returns>The calculated checksum</returns>
        /// <remarks>The checksum is calculated based on the property values sorted in alphabetical order.</remarks>
        public static string CalculateKeyAndPropertiesCheckSum(string keyValue,
            IReadOnlyDictionary<string, string> properties)
        {
            using (var md5 = MD5.Create())
            {
                AddPropertiesToMd5(md5, properties);
                if (keyValue == null)
                {
                    byte[] emptyBytes = {};
                    md5.TransformFinalBlock(emptyBytes, 0, 0);
                }
                else
                {
                    md5.TransformFinalBlock(Encoding.UTF8.GetBytes(keyValue), 0, keyValue.Length);
                }
                return BitConverter.ToString(md5.Hash)
                    .Replace("-", String.Empty);
            }
        }

        /// <summary>
        ///     Calculate a total check sum for all the data values and update the <see cref="CheckSum" /> property with that
        ///     value.
        /// </summary>
        /// <param name="keyValue">This string will be included in the check sum</param>
        /// <param name="recalculateIfAlreadyCalculated">
        ///     True means always calculate the checksum, even if this already has been
        ///     done.
        /// </param>
        public void CalculateCheckSum(string keyValue = null, bool recalculateIfAlreadyCalculated = false)
        {
            if (!recalculateIfAlreadyCalculated && (CheckSum != null)) return;
            using (var md5 = MD5.Create())
            {
                AddThisDataObjectToMd5(md5, new List<Queue<string>>());
                if (keyValue == null)
                {
                    byte[] emptyBytes = {};
                    md5.TransformFinalBlock(emptyBytes, 0, 0);
                }
                else
                {
                    md5.TransformFinalBlock(Encoding.UTF8.GetBytes(keyValue), 0, keyValue.Length);
                }
                CheckSum = BitConverter.ToString(md5.Hash)
                    .Replace("-", String.Empty);
            }
        }

        /// <summary>
        ///     Calculate a total check sum for all the data values except the ones in the black list
        ///     and update the <see cref="CheckSum" /> property with that value.
        /// </summary>
        /// <param name="blackList">A list of the properties that should be excluded from the check sum calculation.</param>
        /// <param name="keyValue">This string will be included in the check sum</param>
        /// <param name="recalculateIfAlreadyCalculated">
        ///     True means always calculate the checksum, even if this already has been
        ///     done.
        /// </param>
        public void CalculateCheckSumWithBlackList(ICollection<string> blackList, string keyValue = null,
            bool recalculateIfAlreadyCalculated = false)
        {
            if (blackList == null)
            {
                throw new ArgumentNullException("blackList", "Use the CalculateCheckSum method.");
            }

            if (!recalculateIfAlreadyCalculated && (CheckSum != null)) return;

            var newBlackList = blackList.Select(name => new Queue<string>(name.Split('.').ToArray()));
            using (var md5 = MD5.Create())
            {
                AddThisDataObjectToMd5(md5, newBlackList);
                if (keyValue == null)
                {
                    byte[] emptyBytes = {};
                    md5.TransformFinalBlock(emptyBytes, 0, 0);
                }
                else
                {
                    md5.TransformFinalBlock(Encoding.UTF8.GetBytes(keyValue), 0, keyValue.Length);
                }
                CheckSum = BitConverter.ToString(md5.Hash)
                    .Replace("-", String.Empty);
            }
        }

        /// <summary>
        ///     Marke the checksum as being ignored
        /// </summary>
        public void IgnoreCheckSum()
        {
            CheckSum = CheckSumToBeIgnored;
        }

        /// <summary>
        ///     Check if the checksum should be ignored
        /// </summary>
        public bool ShouldCheckSumBeIgnored()
        {
            return CheckSum == CheckSumToBeIgnored;
        }

        /// <summary>
        ///     Force a specific checksum
        /// </summary>
        /// <param name="checkSum">The checksum to force set.</param>
        public void ForceSetCheckSum(string checkSum)
        {
            CheckSum = checkSum;
        }

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
                CheckSum = null;
                if (Properties == null) Properties = new CaseInsensitiveDictionary<string>();
                Properties[key] = value;
                if (value != null) return false;
                // Remove the value
                Properties.Remove(key);
                if (Properties.Count != 0) return false;
                Properties = null;
                return true;
            }

            var data = GetNestedProperty(key, true);
            if (data == null)
            {
                CheckSum = null;
                data = new Data();
                NestedProperties[key] = data;
            }

            var remove = SetPropertyValue(value, path);
            if (!remove) return false;
            NestedProperties.Remove(key);
            if (NestedProperties.Count != 0) return false;
            NestedProperties = null;
            return true;
        }

        public string FindDifferingPropertyName(params string[] propertyNamesAndValues)
        {
            string propertyName;
            string expectedValue;
            FindDifferingPropertyName(out propertyName, out expectedValue, ConvertToDictionary(propertyNamesAndValues));
            return propertyName;
        }

        private static CaseInsensitiveDictionary<string> ConvertToDictionary(string[] propertyNamesAndValues)
        {
            var dictionary = new CaseInsensitiveDictionary<string>();

            for (var i = 0; i < propertyNamesAndValues.Length; i += 2)
            {
                dictionary.Add(propertyNamesAndValues[i], propertyNamesAndValues[i + 1]);
            }
            return dictionary;
        }


        public void FindDifferingPropertyName(out string propertyName, out string expectedValue,
            IDictionary<string, string> propertyNamesAndValues)
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

            Properties = source.Properties == null ? null : new CaseInsensitiveDictionary<string>(source.Properties);

            if (source.NestedProperties == null)
            {
                NestedProperties = null;
            }
            else
            {
                NestedProperties = new CaseInsensitiveDictionary<Data>(source.NestedProperties.Count);
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

        public void SetProperties(bool okIfNotExists, IDictionary<string, string> propertyNamesAndValues)
        {
            if (propertyNamesAndValues == null) throw new ArgumentNullException("propertyNamesAndValues");
            if (propertyNamesAndValues.Count < 1) return;

            foreach (var pv in propertyNamesAndValues)
            {
                SetPropertyValue(pv.Key, pv.Value);
            }
        }

        private void AddThisDataObjectToMd5(ICryptoTransform md5, IEnumerable<Queue<string>> blackList)
        {
            var enumerable = blackList as IList<Queue<string>> ?? blackList.ToList();
            var thisLevelBlackList = enumerable.Select(q => q.Peek());
            if (Properties != null)
            {
                var netProperties = Properties
                    .Where(p => !ReferenceEquals(p.Value, null) && !thisLevelBlackList.Contains(p.Key))
                    .OrderBy(p => p.Key);
                AddPropertiesToMd5(md5, netProperties);
            }

            if (NestedProperties == null) return;
            foreach (var nestedProperty in NestedProperties
                .Where(p => !ReferenceEquals(p.Value, null) && !thisLevelBlackList.Contains(p.Key))
                .OrderBy(p => p.Key))
            {
                var nestedBlackList = new List<Queue<string>>();
                foreach (var queue in enumerable)
                {
                    if (nestedProperty.Key != queue.Peek()) continue;
                    var clone = new Queue<string>(queue);
                    clone.Dequeue();
                    nestedBlackList.Add(clone);
                }

                nestedProperty.Value.AddThisDataObjectToMd5(md5, nestedBlackList);
            }
        }

        private static void AddPropertiesToMd5(ICryptoTransform md5,
            IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties == null) return;

            foreach (var property in properties
                .Where(p => !ReferenceEquals(p.Value, null))
                .OrderBy(p => p.Key))
            {
                md5.TransformBlock(Encoding.UTF8.GetBytes(property.Value), 0, property.Value.Length, null, 0);
            }
        }
    }
}