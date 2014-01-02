using System;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class InternalServerErrorException : Fatal
    {
        public const string Type = "InternalServerError";

        public InternalServerErrorException(string message)
            : base(Type, message)
        {
        }

        public InternalServerErrorException(Exception exception)
            : base(Type, "Internal server error", exception)
        {
        }
    }
}
