using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class NotImplementedException : BaseClass
    {
        public NotImplementedException(string message)
            : base(FailureResponse.ErrorTypeEnum.NotImplemented, message)
        {
        }
    }
}
