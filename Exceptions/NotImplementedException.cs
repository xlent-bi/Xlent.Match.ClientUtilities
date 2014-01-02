using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class NotImplementedException : Fatal
    {
        public NotImplementedException(string message)
            : base(FailureResponse.ErrorTypeEnum.NotImplemented, message)
        {
        }
    }
}
