using System;
using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.Messages
{
    /// <summary>
    /// Use this message when you failed to handle a Match <see cref="Request"/>.
    /// </summary>
    [DataContract(Name = "FailureRequest", Namespace = "http://xlentmatch.com/")]
    public class FailureResponse : Response
    {
        public const string Information = "Information";
        public const string Warning = "Warning";
        public const string Error = "Error";
        public const string Fatal = "Fatal";

        public const string NotFound = "NotFound";
        public const string BadRequest = "BadRequest";
        public const string Forbidden = "Forbidden";
        public const string NotImplemented = "NotImplemented";
        public const string Frozen = "Frozen";
        public const string NotAcceptable = "NotAcceptable";
        public const string Gone = "Gone";
        public const string Moved = "Moved";
        public const string InternalServerError = "InternalServerError";

        public enum ErrorLevelEnum { Information, Warning, Error, Fatal };
        public enum ErrorTypeEnum
        {
            NotFound, BadRequest, Forbidden, NotImplemented, Frozen, NotAcceptable, Gone, Moved, InternalServerError
        };

        /// <summary>
        /// The error level (<see cref="Information"/>, <see cref="Warning"/>, <see cref="Error"/>, <see cref="Fatal"/>)
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string ErrorLevel { get; set; }

        /// <summary>
        /// The error type. See <see cref="ErrorTypeEnum"/> for the different values.
        /// Mandatory.
        /// </summary>
        [DataMember]
        public string ErrorType { get; set; }

        /// <summary>
        /// Only used for error type <see cref="Moved"/>.
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="request">The request that this is a response to.</param>
        /// <param name="errorLevel">The error level for the response.</param>
        /// <param name="errorType">The error type.</param>
        public FailureResponse(Request request, ErrorLevelEnum errorLevel, ErrorTypeEnum errorType)
            : base(request, Response.ResponseTypeEnum.Failure)
        {
            ErrorLevel = TranslateErrorLevel(errorLevel);
            ErrorType = TranslateErrorType(errorType);
        }

        /// <summary>
        /// Translate from <see cref="ErrorLevelEnum"/> to a string.
        /// </summary>
        /// <param name="errorLevel">The error level.</param>
        /// <returns>A string representation of the <paramref name="errorLevel"/>.</returns>
        public static string TranslateErrorLevel(ErrorLevelEnum errorLevel)
        {
            switch (errorLevel)
            {
                case ErrorLevelEnum.Information:
                    return Information;
                case ErrorLevelEnum.Warning:
                    return Warning;
                case ErrorLevelEnum.Error:
                    return Error;
                case ErrorLevelEnum.Fatal:
                    return Fatal;
                default:
                    throw new ArgumentException(String.Format("Unknown error level: \"{0}\".", errorLevel));
            }
        }

        /// <summary>
        /// Translate from a string to <see cref="ErrorLevelEnum"/>.
        /// </summary>
        /// <param name="errorLevel">The error level.</param>
        /// <returns>The enumeration value for <paramref name="errorLevel"/>.</returns>
        public static ErrorLevelEnum TranslateErrorLevel(string errorLevel)
        {
            switch (errorLevel)
            {
                case Information:
                    return ErrorLevelEnum.Information;
                case Warning:
                    return ErrorLevelEnum.Warning;
                case Error:
                    return ErrorLevelEnum.Error;
                case Fatal:
                    return ErrorLevelEnum.Fatal;
                default:
                    throw new ArgumentException(String.Format("Unknown error level: \"{0}\".", errorLevel));
            }
        }

        /// <summary>
        /// Translate from <see cref="ErrorTypeEnum"/> to a string.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <returns>A string representation of the <paramref name="errorType"/>.</returns>
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
                case ErrorTypeEnum.InternalServerError:
                    return InternalServerError;
                default:
                    throw new ArgumentException(String.Format("Unknown error type: \"{0}\".", errorType));
            }
        }

        /// <summary>
        /// Translate from a string to <see cref="ErrorTypeEnum"/>.
        /// </summary>
        /// <param name="errorType">The error type.</param>
        /// <returns>The enumeration value for <paramref name="errorType"/>.</returns>
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
                case InternalServerError:
                    return ErrorTypeEnum.InternalServerError;
                default:
                    throw new ArgumentException(String.Format("Unknown error type: \"{0}\".", errorType));
            }
        }
    }
}
