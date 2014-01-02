using System;
using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class InternalServerErrorException : Fatal
    {
        public InternalServerErrorException(string message)
            : base(FailureResponse.ErrorTypeEnum.InternalServerError, message)
        {
        }

        public InternalServerErrorException(Exception exception)
            : base(FailureResponse.ErrorTypeEnum.InternalServerError, "Internal server error", exception)
        {
        }
    }
}
