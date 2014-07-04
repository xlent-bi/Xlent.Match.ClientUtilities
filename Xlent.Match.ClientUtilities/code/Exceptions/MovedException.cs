using System;
using Xlent.Match.ClientUtilities.MatchObjectModel;
using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class MovedException : MatchException
    {
        public MovedException(string newKeyValue)
            : base(FailureResponse.ErrorTypeEnum.Moved, String.Format("Redirection to new key value {0}", newKeyValue))
        {
            NewKeyValue = newKeyValue;
        }

        public MovedException(Key oldKey, string newKeyValue)
            : base(
                FailureResponse.ErrorTypeEnum.Moved,
                String.Format("Redirection for {0} to new key value {1}", oldKey, newKeyValue))
        {
            NewKeyValue = newKeyValue;
        }

        public string NewKeyValue { get; private set; }
    }
}