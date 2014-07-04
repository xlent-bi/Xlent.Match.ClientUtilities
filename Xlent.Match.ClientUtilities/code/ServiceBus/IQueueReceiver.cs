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
        void OnMessageAsync(Func<BrokeredMessage,Task> asyncAction, OnMessageOptions onMessageOptions);
        void Disable();
        void Activate();
        Task FlushAsync();
        Task SafeAbandonAsync(BrokeredMessage message);
        Task SafeCompleteAsync<T>(BrokeredMessage message, T interpretedMessage);
        Task SafeDeadLetterAsync(BrokeredMessage message);
        Task ForEachMessageAsync(Func<BrokeredMessage, Task> actionAsync);
    }
}