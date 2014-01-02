using System;
using System.Runtime.Serialization;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities.Messages
{
    /// <summary>
    /// This message is used by Match to send requests to the client adapters.
    /// </summary>
    [DataContract]
    public class Request
    {
        public const string Create = "Create";
        public const string Update = "Update";
        public const string Get = "Get";

        public enum RequestTypeEnum { Create, Update, Get };
        
        /// <summary>
        /// The type of the request.
        /// Mandatory, one of <see cref="Request.Create"/>,  <see cref="Request.Update"/>
        /// and  <see cref="Request.Get"/>.
        /// </summary>
        [DataMember]
        public string RequestType { get; set; }

        /// <summary>
        /// The internal Match id for the process that this message is part of.
        /// Mandatory.
        /// Use this process id when you make your response.
        /// </summary>
        [DataMember]
        public string ProcessId { get; set; }

        /// <summary>
        /// The client that the request is directed to. You should only subscribe to your own messages.
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string ClientName { get; set; }

        /// <summary>
        /// The entity within your client that the request is referring to.
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string EntityName { get; set; }

        /// <summary>
        /// The identity of the object that the request is referring to.
        /// Mandatory for requests of type <see cref="Request.Update"/> and <see cref="Request.Get"/>.
        /// </summary>
        [DataMember]
        public string KeyValue { get; set; }

        /// <summary>
        /// The internal Match identity of the object that the request is referring to.
        /// Mandatory for requests of type <see cref="Request.Create"/>.
        /// </summary>
        [DataMember]
        public string MatchId { get; set; }

        /// <summary>
        /// The data for the request.
        /// Mandatory for requests of type <see cref="Request.Create"/> and <see cref="Request.Update"/>.
        /// </summary>
        [DataMember]
        public MatchObject MatchObject { get; set; }

        public Request(RequestTypeEnum requestType)
        {
            RequestType = TranslateRequestType(requestType);
        }

        /// <summary>
        /// Translate from <see cref="RequestTypeEnum"/> to a string.
        /// </summary>
        /// <param name="requestType">The request type.</param>
        /// <returns>A string representation of the <paramref name="requestType"/>.</returns>
        public static string TranslateRequestType(RequestTypeEnum requestType)
        {
            switch (requestType)
            {
                case RequestTypeEnum.Create:
                    return Create;
                case RequestTypeEnum.Update:
                    return Update;
                case RequestTypeEnum.Get:
                    return Get;
                default:
                    throw new ArgumentException(String.Format("Unknown request type: \"{0}\".", requestType));
            }
        }

        /// <summary>
        /// Translate from a string to <see cref="RequestTypeEnum"/>.
        /// </summary>
        /// <param name="requestType">The request type.</param>
        /// <returns>The enumeration value for <paramref name="requestType"/>.</returns>
        public static RequestTypeEnum TranslateRequestType(string requestType)
        {
            switch (requestType)
            {
                case Create:
                    return RequestTypeEnum.Create;
                case Update:
                    return RequestTypeEnum.Update;
                case Get:
                    return RequestTypeEnum.Get;
                default:
                    throw new ArgumentException(String.Format("Unknown request type: \"{0}\".", requestType));
            }
        }

        public static string SubscriptionName(string clientName, string entityName)
        {
                return string.Join(".", clientName, entityName);
        }
    }
}
