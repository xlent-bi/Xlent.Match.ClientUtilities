using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    /// <summary>
    /// Exceptions of this category indicates that the caller made an error.
    /// </summary>
    public abstract class Error : BaseClass
    {
        protected Error(FailureResponse.ErrorTypeEnum errorType, string message)
            : base(FailureResponse.ErrorLevelEnum.Error, errorType, message)
        {
        }
    }
}