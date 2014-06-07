using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities.Messages
{
    /// <summary>
    /// Abstract class for a responses to a Match <see cref="Request"/>.
    /// </summary>
    [DataContract(Name = "Response", Namespace = "http://xlentmatch.com/")]
    public abstract class Response
    {
        public const string Success = "Success";
        public const string Failure = "Failure";

        public enum ResponseTypeEnum { Success, Failure };

        /// <summary>
        /// The type of the response. Mandatory, one of <see cref="Response.Success"/>,
        /// and <see cref="Response.Failure"/>.
        /// </summary>
        [DataMember]
        public string ResponseTypeAsString { get; private set; }

        public ResponseTypeEnum ResponseType { get { return TranslateResponseType(ResponseTypeAsString); } }

        /// <summary>
        /// The type of the request.
        /// Mandatory, one of <see cref="Request.Create"/>,  <see cref="Request.Update"/>
        /// and  <see cref="Request.Get"/>.
        /// </summary>
        [DataMember]
        public string RequestTypeAsString { get; private set; }
        public Request.RequestTypeEnum RequestType { get { return Request.TranslateRequestType(RequestTypeAsString); } }

        /// <summary>
        /// The <see cref="Request.ProcessId"/>.
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string ProcessId { get; set; }

        /// <summary>
        /// The key for the response.
        /// Mandatory.
        /// </summary>
        [DataMember]
        public Key Key { get; set; }

        /// <summary>
        /// The data for the response.
        /// Mandatory for requests of type <see cref="Request.Get"/>.
        /// </summary>
        [DataMember]
        public Data Data { get; set; }

        /// <summary>
        /// The <see cref="Request.ClientName"/>.
        /// Mandatory.
        /// </summary>
        public string ClientName { get { return Key.ClientName; } }

        /// <summary>
        /// The <see cref="Request.EntityName"/>.
        /// Mandatory.
        /// </summary>
        public string EntityName { get { return Key.EntityName; } }

        /// <summary>
        /// For requests of type <see cref="Request.Update"/> and <see cref="Request.Get"/>
        /// this property must have the same value as  <see cref="Request.KeyValue"/>.
        /// For requests of type <see cref="Request.Create"/> this property should have the identity of the
        /// object that was created (or found to be already existing).
        /// Mandatory.
        /// </summary>
        public string KeyValue { get { return Key.Value; } }

        /// <summary>
        /// For requests of type <see cref="Request.Create"/>
        /// this property must have the same value as  <see cref="Request.MatchId"/>.
        /// </summary>
        public string MatchId { get { return Key.MatchId; } }

        /// <summary>
        /// Constructor for this class.
        /// </summary>
        /// <param name="request">The request that this response is associated to.</param>
        /// <param name="responseType">The type of response.</param>
        protected Response(Request request, ResponseTypeEnum responseType)
        {

            ResponseTypeAsString = TranslateResponseType(responseType);
            RequestTypeAsString = request.RequestTypeAsString;
            ProcessId = request.ProcessId;
            Key = request.Key;
        }

        /// <summary>
        /// Translate from <see cref="ResponseTypeEnum"/> to a string.
        /// </summary>
        /// <param name="responseType">The response type.</param>
        /// <returns>A string representation of the <paramref name="responseType"/>.</returns>
        public static string TranslateResponseType(ResponseTypeEnum responseType)
        {
            switch (responseType)
            {
                case ResponseTypeEnum.Success:
                    return Success;
                case ResponseTypeEnum.Failure:
                    return Failure;
                default:
                    throw new ArgumentException(String.Format("Unknown response type: \"{0}\".", responseType));
            }
        }

        /// <summary>
        /// Translate from a string to <see cref="ResponseTypeEnum"/>.
        /// </summary>
        /// <param name="responseType">The response type.</param>
        /// <returns>The enumeration value for <paramref name="responseType"/>.</returns>
        public static ResponseTypeEnum TranslateResponseType(string responseType)
        {
            switch (responseType)
            {
                case Success:
                    return ResponseTypeEnum.Success;
                case Failure:
                    return ResponseTypeEnum.Failure;
                default:
                    throw new ArgumentException(String.Format("Unknown response type: \"{0}\".", responseType));
            }
        }

        public override string ToString()
        {
            return String.Format("{0} response for request {1} for key {2}.", ResponseTypeAsString, RequestTypeAsString, Key);
        }
    }
}
