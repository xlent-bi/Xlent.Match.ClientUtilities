using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class MovedException : Warning
    {
        public string NewKeyValue { get; private set; }

        public MovedException(string newKeyValue)
            : base(FailureResponse.ErrorTypeEnum.Moved, "Redirection.")
        {
            NewKeyValue = newKeyValue;
        }
    }
}
