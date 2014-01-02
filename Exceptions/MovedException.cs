namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class MovedException : Warning
    {
        public const string Type = "Moved";

        public string NewKeyValue { get; private set; }

        public MovedException(string newKeyValue)
            : base(Type, "Redirection.")
        {
            NewKeyValue = newKeyValue;
        }
    }
}
