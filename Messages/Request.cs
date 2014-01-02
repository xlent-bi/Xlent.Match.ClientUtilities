using System;
using System.Runtime.Serialization;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities.Messages
{
    [DataContract]
    public class Request
    {
        [DataMember]
        public string RequestType { get; set; }

        [DataMember]
        public string ProcessId { get; set; }

        [DataMember]
        public string ClientName { get; set; }

        [DataMember]
        public string EntityName { get; set; }

        [DataMember]
        public string KeyValue { get; set; }

        [DataMember]
        public string MatchId { get; set; }

        [DataMember]
        public MatchObject MatchObject { get; set; }

        public Request(string requestType)
        {
            RequestType = requestType;
        }

        public string SubscriptionName
        {
            get
            {
                return string.Join(".", ClientName, EntityName);
            }
        }
    }
}
