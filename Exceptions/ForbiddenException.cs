namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class ForbiddenException : Error
    {
        public const string Type = "Forbidden";

        public ForbiddenException(string message)
            : base(Type, message)
        {
        }
    }
}
