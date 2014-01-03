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

        protected BaseClass(FailureResponse.ErrorTypeEnum errorType, string message)
            : base(message)
        {
            ErrorType = errorType;
        }

        protected BaseClass(FailureResponse.ErrorTypeEnum errorType, string message, Exception exception)
            : base(message, exception)
        {
            ErrorType = errorType;
        }
    }
}