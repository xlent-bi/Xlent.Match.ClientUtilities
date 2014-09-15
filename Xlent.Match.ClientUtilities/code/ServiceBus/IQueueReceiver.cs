using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueReceiver
    {
        string Name { get; }
        Task<long> GetLengthAsync();
        bool IsPeekReceiveMode { get; }
        long GetLength();
        Task<bool> IsEmptyAsync();
        bool IsEmpty();
        Task<BrokeredMessage> NonBlockingReceiveAsync();
        Task<BrokeredMessage> BlockingReceiveAsync();
        void OnMessageAsync(Func<BrokeredMessage, Task> asyncAction, OnMessageOptions onMessageOptions);
        Task DisableAsync();
        Task ActivateAsync();
        Task SetLockDurationAsync(TimeSpan durationTimeSpan);
        Task FlushAsync();
        Task SafeAbandonAsync(BrokeredMessage message);
        Task SafeCompleteAsync<T>(BrokeredMessage message, T interpretedMessage);
        Task SafeDeadLetterAsync(BrokeredMessage message);
        Task ForEachMessageAsyncUsingReceiveAndDeleteMode(Func<BrokeredMessage, Task> actionAsync);
    }
}