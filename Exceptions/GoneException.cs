namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class GoneException : Error
    {
        public const string Type = "Gone";

        public GoneException(string clientName, string entityName, string keyValue)
            : base(Type, string.Format("The object {0}/{1}/{2} has been permanently removed.", clientName, entityName, keyValue))
        {
        }
    }
}
