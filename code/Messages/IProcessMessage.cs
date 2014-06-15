using System.Runtime.Serialization;

namespace Xlent.Match.ClientUtilities.Messages
{
    public interface IProcessMessage : IKeyMessage
    {
        /// <summary>
        /// The internal Match id for the process that this message is part of.
        /// Mandatory.
        /// Use this process id when you make your response.
        /// </summary>
        string ProcessId { get; }
    }
}