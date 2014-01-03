using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class FrozenException : BaseClass
    {
        public FrozenException(string clientName, string entityName, string keyValue)
            : base(FailureResponse.ErrorTypeEnum.Frozen, string.Format("The object {0}/{1}/{2} has been frozen.", clientName, entityName, keyValue))
        {
        }
    }
}
