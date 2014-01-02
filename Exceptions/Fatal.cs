using System;
using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    /// <summary>
    /// Exceptions of this category indicates that the adapter has failed.
    /// </summary>
    public abstract class Fatal : BaseClass
    {
        protected Fatal(FailureResponse.ErrorTypeEnum errorType, string message)
            : base(FailureResponse.ErrorLevelEnum.Fatal, errorType, message)
        {
        }

        protected Fatal(FailureResponse.ErrorTypeEnum errorType, string message, Exception exception)
            : base(FailureResponse.ErrorLevelEnum.Fatal, errorType, message, exception)
        {
        }
    }
}
