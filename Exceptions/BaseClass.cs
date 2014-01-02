using System;

using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    /// <summary>
    /// Exceptions of this category indicates that the caller made an error.
    /// </summary>
    public abstract class BaseClass : Exception
    {
        public FailureResponse.ErrorTypeEnum ErrorType { get; private set; }
        public FailureResponse.ErrorLevelEnum ErrorLevel { get; private set; }

        protected BaseClass(FailureResponse.ErrorLevelEnum errorLevel, FailureResponse.ErrorTypeEnum errorType, string message)
            : base(message)
        {
            ErrorLevel = errorLevel;
            ErrorType = errorType;
        }

        protected BaseClass(FailureResponse.ErrorLevelEnum errorLevel, FailureResponse.ErrorTypeEnum errorType, string message, Exception exception)
            : base(message, exception)
        {
            ErrorLevel = errorLevel;
            ErrorType = errorType;
        }
    }
}