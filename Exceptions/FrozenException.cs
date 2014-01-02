namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class FrozenException : Warning
    {
        public const string Type = "Frozen";

        public FrozenException(string clientName, string entityName, string keyValue)
            : base(Type, string.Format("The object {0}/{1}/{2} has been frozen.", clientName, entityName, keyValue))
        {
        }
    }
}
