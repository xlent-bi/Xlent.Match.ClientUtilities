using System;
using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.Messages
{
    [DataContract]
    public class FailureResponse : Response
    {
        [DataMember]
        public string ErrorLevel { get; set; }

        [DataMember]
        public string ErrorType { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string Message { get; set; }

        public FailureResponse(Request request)
            : base("Failure", request)
        {
        }
    }
}
