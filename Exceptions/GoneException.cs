using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class GoneException : Error
    {
        public GoneException(string clientName, string entityName, string keyValue)
            : base(FailureResponse.ErrorTypeEnum.Gone, string.Format("The object {0}/{1}/{2} has been permanently removed.", clientName, entityName, keyValue))
        {
        }
    }
}
