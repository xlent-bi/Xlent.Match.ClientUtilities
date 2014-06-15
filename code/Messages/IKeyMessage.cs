using System.Runtime.Serialization;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities.Messages
{
    public interface IKeyMessage
    {
        Key Key { get; }
    }
}