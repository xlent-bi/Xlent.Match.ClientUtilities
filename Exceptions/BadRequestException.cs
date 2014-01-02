namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class BadRequestException : Error
    {
        public const string Type = "BadReqest";

        public BadRequestException(string message)
            : base(Type, message)
        {
        }
    }
}
