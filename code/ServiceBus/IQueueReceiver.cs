using System;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueReceiver
    {
        string Name { get; }
        BrokeredMessage NonBlockingReceive();
        BrokeredMessage BlockingReceive();
        void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions);
    }
}