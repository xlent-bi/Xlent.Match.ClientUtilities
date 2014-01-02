namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class NotFoundException : Error
    {
        public const string Type = "NotFound";

        public NotFoundException(string clientName, string entityName, string keyValue)
            : base(Type, string.Format("Could not find {0}/{1}/{2}", clientName, entityName, keyValue))
        {
        }
    }
}
