using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public interface IQueueSender
    {
        string Name { get; }
        long GetLength();
        void Send(BrokeredMessage message);
        void Send<T>(T message, IDictionary<string, object> properties = null);
        Task ResendAndCompleteAsync(BrokeredMessage message);
        Task FlushAsync();
        Task ForEachMessageAsync(Func<BrokeredMessage, Task> actionAsync);
    }
}