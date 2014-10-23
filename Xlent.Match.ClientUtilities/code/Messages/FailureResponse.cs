using System;
using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.Messages
{
    /// <summary>
    ///     Use this message when you failed to handle a Match <see cref="Request" />.
    /// </summary>
    [DataContract(Name = "FailureRequest", Namespace = "http://xlentmatch.com/")]
    public class FailureResponse : Response
    {
        public enum ErrorTypeEnum
        {
            NotFound,
            BadRequest,
            Forbidden,
            NotImplemented,
            Frozen,
            NotAcceptable,
            Gone,
            Moved,
            HasBeenUpdated,
            InternalServerError,
            AdapterDidNotHandleException
        };

        public const string NotFound = "NotFound";
        public const string BadRequest = "BadRequest";
        public const string Forbidden = "Forbidden";
        public const string NotImplemented = "NotImplemented";
        public const string Frozen = "Frozen";
        public const string NotAcceptable = "NotAcceptable";
        public const string Gone = "Gone";
        public const string Moved = "Moved";
        public const string HasBeenUpdated = "HasBeenUpdated";
        public const string InternalServerError = "InternalServerError";

        /// <summary>
        ///     The constructor.
        /// </summary>
        /// <param name="request">The request that this is a response to.</param>
        /// <param name="errorType">The error type.</param>
        public FailureResponse(Request request, ErrorTypeEnum errorType)
            : base(request, ResponseTypeEnum.Failure)
        {
            ErrorTypeAsString = TranslateErrorType(errorType);
        }

        /// <summary>
        ///     The error type. See <see cref="ErrorTypeEnum" /> for the different values.
        ///     Mandatory.
        /// </summary>
        [DataMember]
        public string ErrorTypeAsString { get; private set; }

        public ErrorTypeEnum ErrorType
        {
            get { return TranslateErrorType(ErrorTypeAsString); }
        }

        /// <summary>
        ///     Only used for error type <see cref="Moved" />.
        ///     Contains the new KeyValue (where the information has moved to).
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        ///     The error message.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        ///     Translate from <see cref="ErrorTypeEnum" /> to a string.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <returns>A string representation of the <paramref name="errorType" />.</returns>
        public static string TranslateErrorType(ErrorTypeEnum errorType)
        {
            switch (errorType)
            {
                case ErrorTypeEnum.NotFound:
                    return NotFound;
                case ErrorTypeEnum.BadRequest:
                    return BadRequest;
                case ErrorTypeEnum.Forbidden:
                    return Forbidden;
                case ErrorTypeEnum.NotImplemented:
                    return NotImplemented;
                case ErrorTypeEnum.Frozen:
                    return Frozen;
                case ErrorTypeEnum.NotAcceptable:
                    return NotAcceptable;
                case ErrorTypeEnum.Gone:
                    return Gone;
                case ErrorTypeEnum.Moved:
                    return Moved;
                case ErrorTypeEnum.HasBeenUpdated:
                    return HasBeenUpdated;
                case ErrorTypeEnum.InternalServerError:
                    return InternalServerError;
                default:
                    throw new ArgumentException(String.Format("Unknown error type: \"{0}\".", errorType));
            }
        }

        /// <summary>
        ///     Translate from a string to <see cref="ErrorTypeEnum" />.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <returns>The enumeration value for <paramref name="errorType" />.</returns>
        public static ErrorTypeEnum TranslateErrorType(string errorType)
        {
            switch (errorType)
            {
                case NotFound:
                    return ErrorTypeEnum.NotFound;
                case BadRequest:
                    return ErrorTypeEnum.BadRequest;
                case Forbidden:
                    return ErrorTypeEnum.Forbidden;
                case NotImplemented:
                    return ErrorTypeEnum.NotImplemented;
                case Frozen:
                    return ErrorTypeEnum.Frozen;
                case NotAcceptable:
                    return ErrorTypeEnum.NotAcceptable;
                case Gone:
                    return ErrorTypeEnum.Gone;
                case Moved:
                    return ErrorTypeEnum.Moved;
                case HasBeenUpdated:
                    return ErrorTypeEnum.HasBeenUpdated;
                case InternalServerError:
                    return ErrorTypeEnum.InternalServerError;
                default:
                    throw new ArgumentException(String.Format("Unknown error type: \"{0}\".", errorType));
            }
        }

        public override string ToString()
        {
            return String.Format("[FailureReponse {0} {1}]", ErrorTypeAsString, base.ToString());
        }
    }
}