using System;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    /// <summary>
    /// Exceptions of this category indicates that the caller made an error.
    /// </summary>
    public abstract class BaseClass : Exception
    {
        public string ErrorType { get; private set; }
        public string ErrorLevel { get; private set; }

        protected BaseClass(string errorLevel, string errorType, string message)
            : base(message)
        {
            ErrorLevel = errorLevel;
            ErrorType = errorType;
        }

        protected BaseClass(string errorLevel, string errorType, string message, Exception exception)
            : base(message, exception)
        {
            ErrorLevel = errorLevel;
            ErrorType = errorType;
        }
    }
}