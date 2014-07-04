using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class ForbiddenException : MatchException
    {
        public ForbiddenException(string message)
            : base(FailureResponse.ErrorTypeEnum.Forbidden, message)
        {
        }
    }
}
