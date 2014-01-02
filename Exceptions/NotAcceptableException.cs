using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class NotAcceptableException : Error
    {
        public NotAcceptableException(string message)
            : base(FailureResponse.ErrorTypeEnum.NotAcceptable, message)
        {
        }
    }
}
