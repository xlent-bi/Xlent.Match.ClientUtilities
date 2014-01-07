using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class NotFoundException : BaseClass
    {
        public NotFoundException(string clientName, string entityName, string keyValue)
            : base(FailureResponse.ErrorTypeEnum.NotFound, string.Format("Could not find {0}/{1}/{2}", clientName, entityName, keyValue))
        {
        }
    }
}
