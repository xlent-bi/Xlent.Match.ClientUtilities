using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueReceiver
    {
        string Name { get; }
        long GetLength();
        BrokeredMessage NonBlockingReceive();
        BrokeredMessage BlockingReceive();
        void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions);
        void Disable();
        void Activate();
        Task FlushAsync();
    }
}