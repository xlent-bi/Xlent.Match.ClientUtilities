using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities.Messages
{
    [DataContract]
    public abstract class Response
    {
        [DataMember]
        public string ResponseType { get; set; }

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

        protected Response(string responseType, Request request)
        {
            ResponseType = responseType;
            ProcessId = request.ProcessId;
            ClientName = request.ClientName;
            EntityName = request.EntityName;
            RequestType = request.RequestType;
            KeyValue = request.KeyValue;
            MatchId = request.MatchId;
            MatchObject = request.MatchObject;
        }
    }
}
