using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities
{
    public class MatchObjectControl
    {
        public MatchObject MatchObject { get; private set; }

        public MainKey MainKey { get { return MatchObject.MainKey; } }

        public MatchObjectControl(string clientName, string entity, string id, string matchId)
        {
            MatchObject = new MatchObject
            {
                MainKey = new MainKey
                {
                    ClientName = clientName,
                    EntityName = entity,
                    MatchId = matchId,
                    Value = id
                },
                ObjectData = new ObjectData()
            };

            if (MatchObject.ObjectData == null)
                MatchObject.ObjectData = new ObjectData();
            if (MatchObject.ObjectData.Properties == null)
                MatchObject.ObjectData.Properties = new List<Property>();
            if (MatchObject.ObjectData.NestedProperties == null)
                MatchObject.ObjectData.NestedProperties = new List<NestedProperty>();
        }

        public MatchObjectControl(string client, string entity, string id)
            : this(client, entity, id, null)
        {
        }

        public MatchObjectControl(MatchObject matchObject)
        {
            MatchObject = matchObject;
        }

        public MatchObjectControl(MainKey mainKey, ObjectData objectData)
        {
            MatchObject = new MatchObject
            {
                MainKey = mainKey,
                ObjectData = (objectData ?? new ObjectData())
            };
        }


        public string GetPropertyValue(string name)
        {
            return GetPropertyValue(name, false);
        }

        public string GetPropertyValue(string name, bool okIfNotExist)
        {
            Debug.Assert(MatchObject != null);
            var property = GetProperty(name, okIfNotExist);
            return null == property ? null : property.Value;
        }

        /// <summary>
        /// Update the current matchObject with the values in another matchObject.
        /// </summary>
        /// <param name="matchObject"></param>
        public void Update(MatchObject matchObject)
        {
            Debug.Assert(MatchObject != null);
            foreach (var property in matchObject.ObjectData.Properties)
            {
                SetProperties(true, property.Name, property.Value);
            }
        }
        
        private List<Property> _regularProperties;
        /// <summary>
        /// A subset of <see cref="MatchObject.DataObject.Properties"/>; the regular properties, i.e. properties that are not keys or references.
        /// </summary>
        public List<Property> RegularProperties
        {
            get
            {
                if (null == _regularProperties)
                {
                    UpdatePropertyLists();
                }
                return _regularProperties;
            }
        }


        private List<Property> _keyProperties;
        /// <summary>
        /// A subset of <see cref="Properties"/>; the properties that are keys for this object.
        /// </summary>
        public List<Property> KeyProperties
        {
            get
            {
                if (null == _keyProperties)
                {
                    UpdatePropertyLists();
                }
                return _keyProperties;
            }
        }

        private List<Property> _referenceProperties;
        /// <summary>
        /// A subset of <see cref="Properties"/>; the properties that are references to other objects.
        /// </summary>
        public List<Property> ReferenceProperties
        {
            get
            {
                if (null == _referenceProperties)
                {
                    UpdatePropertyLists();
                }
                return _referenceProperties;
            }
        }

        /// <summary>
        /// Call this to invalidate the three property lists; <see cref="RegularProperties"/>, 
        /// <see cref="KeyProperties"/>, <see cref="ReferenceProperties"/>.
        /// </summary>
        /// <remarks>Call this after you have changed the property list <see cref="Properties"/>.</remarks>
        public void InvalidateProperties()
        {
            _regularProperties = null;
            _keyProperties = null;
            _referenceProperties = null;
        }

        /// <summary>
        /// Call the three property lists; <see cref="RegularProperties"/>, 
        /// <see cref="KeyProperties"/>, <see cref="ReferenceProperties"/>.
        /// </summary>
        private void UpdatePropertyLists()
        {
            _regularProperties = new List<Property>();
            _keyProperties = new List<Property>();
            _referenceProperties = new List<Property>();
            if (null != MatchObject.ObjectData.Properties)
            {
                foreach (var property in MatchObject.ObjectData.Properties)
                {
                    if (property.IsKey) _keyProperties.Add(property);
                    else if (property.IsRegular) _regularProperties.Add(property);
                    else _regularProperties.Add(property);
                }
            }
        }

        #region Test
        public void SetProperties(bool okIfNotExists, params string[] arguments)
        {
            List<Property> properties = null;
            if (null == MatchObject.ObjectData.Properties)
            {
                properties = new List<Property>(arguments.Length / 2);
            }
            else
            {
                properties = new List<Property>(MatchObject.ObjectData.Properties);
            }

            for (int i = 0; i < arguments.Length; i += 2)
            {
                string name = arguments[i];
                string value = arguments[i + 1];
                Property property = GetProperty(name, true);
                if (null == property)
                {
                    property = new Property(name, value);
                    properties.Add(property);
                }
                else
                {
                    property.Value = value;
                }
            }
            MatchObject.ObjectData.Properties = properties;
            InvalidateProperties();
        }

        public Property GetProperty(string name, bool okIfNotExists)
        {
            if (null != MatchObject.ObjectData.Properties)
            {
                foreach (Property property in MatchObject.ObjectData.Properties)
                {
                    if (property.Name == name) return property;
                }
            }
            if (okIfNotExists) return null;
            throw new ApplicationException();
        }

        public int FindDifferingProperty(params string[] arguments)
        {
            for (var i = 0; i < arguments.Length; i += 2)
            {
                var name = arguments[i];
                var value = arguments[i + 1];
                var property = GetProperty(name, true);
                if ((null == value) && (null == property)) continue;
                if ((null == property) || (value != property.Value))
                {
                    return i + 1;
                }
            }
            return 0;
        }
        #endregion
    }
}
