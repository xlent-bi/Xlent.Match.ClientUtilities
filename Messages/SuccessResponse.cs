using System;
using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.Messages
{
    [DataContract]
    public class SuccessResponse : Response
    {
        public SuccessResponse(Request request)
            : base("Success", request)
        {
        }
    }
}
