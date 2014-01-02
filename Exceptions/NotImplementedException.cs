namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class NotImplementedException : Fatal
    {
        public const string Type = "NotImplemented";

        public NotImplementedException(string message)
            : base(Type, message)
        {
        }
    }
}
