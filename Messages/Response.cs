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
        /// The type of the response. Mandatory, one of <see cref="Event.Success"/>,
        /// and <see cref="Event.Failure"/>.
        [DataMember]
        public string ResponseType { get; set; }

        /// <summary>
        /// The type of the request.
        /// Mandatory, one of <see cref="Request.Create"/>,  <see cref="Request.Update"/>
        /// and  <see cref="Request.Get"/>.
        /// </summary>
        [DataMember]
        public string RequestType { get; set; }

        /// <summary>
        /// The <see cref="Request.ProcessId"/>.
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string ProcessId { get; set; }

        /// <summary>
        /// The <see cref="Request.ClientName"/>.
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string ClientName { get; set; }

        /// <summary>
        /// The <see cref="Request.EntityName"/>.
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string EntityName { get; set; }

        /// <summary>
        /// For requests of type <see cref="Request.Update"/> and <see cref="Request.Get"/>
        /// this property must have the same value as  <see cref="Request.KeyValue"/>.
        /// For requests of type <see cref="Request.Create"/> this property should have the identity of the
        /// object that was created (or found to be already existing).
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string KeyValue { get; set; }

        /// <summary>
        /// For requests of type <see cref="Request.Create"/>
        /// this property must have the same value as  <see cref="Request.MatchId"/>.
        /// </summary>
        [DataMember]
        public string MatchId { get; set; }

        /// <summary>
        /// The data for the response.
        /// Mandatory for requests of type <see cref="Request.Get"/>.
        /// </summary>
        [DataMember]
        public MatchObject MatchObject { get; set; }

        /// <summary>
        /// Constructor for this class.
        /// </summary>
        /// <param name="request">The request that this response is associated to.</param>
        /// <param name="responseType">The type of response.</param>
        /// <remarks>
        /// The only property that is not set by this constructor is the <see cref="MatchObject"/> property.</remarks>
        protected Response(Request request, ResponseTypeEnum responseType)
        {
            ResponseType = TranslateResponseType(responseType);

            ProcessId = request.ProcessId;
            ClientName = request.ClientName;
            EntityName = request.EntityName;
            RequestType = request.RequestType;
            KeyValue = request.KeyValue;
            MatchId = request.MatchId;
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
    }
}
