using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class BadRequestException : BaseClass
    {
        public BadRequestException(string message)
            : base(FailureResponse.ErrorTypeEnum.BadRequest, message)
        {
        }
    }
}
