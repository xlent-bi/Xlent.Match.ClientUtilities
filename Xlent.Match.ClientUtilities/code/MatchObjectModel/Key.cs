using System;
using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.MatchObjectModel
{

    /// <summary>
    /// The identifier information for the MatchObject.
    /// </summary>
    [DataContract(Name = "Key", Namespace = "http://xlentmatch.com/")]
    public class Key
    {
        /// <summary>
        /// The name of the client that has this object
        /// </summary>
        [DataMember]
        public string ClientName { get; set; }

        /// <summary>
        /// The name of the entity that this object belongs to
        /// </summary>
        [DataMember]
        public string EntityName { get; set; }

        /// <summary>
        /// The identity in the client for this object.
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// The identity in XlentMatch for this object.
        /// </summary>
        [DataMember]
        public string MatchId { get; set; }

        public string CompactKey
        {
            get { return String.Join("/", ClientName, EntityName, Value ?? MatchId); }
        }

        public Key(string clientName, string entityName, string value, string matchId = null)
        {
            ClientName = clientName;
            EntityName = entityName;
            Value = value;
            MatchId = matchId;
        }

        public override bool Equals(object otherKey)
        {
            var key = otherKey as Key;
            if (key == null) return false;

            if ((String.Compare(key.ClientName, ClientName, StringComparison.InvariantCultureIgnoreCase) != 0)
                || (String.Compare(key.EntityName, EntityName, StringComparison.InvariantCultureIgnoreCase) != 0)) return false;
            if ((key.MatchId != null) && (key.MatchId == MatchId)) return true;
            return key.Value == Value;
        }

        public override int GetHashCode()
        {
            return ClientName.GetHashCode() ^ EntityName.GetHashCode() ^ Value.GetHashCode();
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(Value) ?
                string.Format("{0}/{1} ({2})", ClientName, EntityName, MatchId)
                : string.Format("{0}/{1}/{2}", ClientName, EntityName, Value);
        }
    }
}
