using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueSender
    {
        string Name { get; }
        Task<long> GetLengthAsync();
        long GetLength();
        Task<bool> IsEmptyAsync();
        bool IsEmpty();
        Task SendAsync<T>(T message, IDictionary<string, object> properties = null);
        Task ResendAndCompleteAsync(BrokeredMessage message, IQueueReceiver queueReceiver);
        Task FlushAsync();
        Task ForEachMessageAsyncUsingReceiveAndDeleteMode(Func<BrokeredMessage, Task> actionAsync);
    }
}