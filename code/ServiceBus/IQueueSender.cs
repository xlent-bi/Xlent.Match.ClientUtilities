using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueSender
    {
        string Name { get; }
        long GetLength();
        void Send(BrokeredMessage message);
        void Send<T>(T message, IDictionary<string, object> properties = null);
        void ResendAndComplete(BrokeredMessage message);
    }
}