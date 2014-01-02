namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class NotAcceptableException : Error
    {
        public const string Type = "NotAcceptable";

        public NotAcceptableException(string message)
            : base(Type, message)
        {
        }
    }
}
