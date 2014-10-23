using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Xlent.Match.ClientUtilities.Logging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Queue : BaseClass, IQueueSender, IQueueReceiver, IQueueAdministrator
    {
        private readonly ReceiveMode _receiveMode;

        public Queue(string connectionStringName, string name, ReceiveMode receiveMode = ReceiveMode.PeekLock)
            : base(connectionStringName)
        {
            _receiveMode = receiveMode;
            Name = name;
            // Create a new Queue with custom settings
            SafeCreateQueueAsync(name).Wait();
            Client = QueueClient.CreateFromConnectionString(ConnectionString, name, receiveMode);
        }

        private QueueClient Client { get; set; }

        public async Task DeleteAsync()
        {
            await RetryPolicy.ExecuteAsync(() => NamespaceManager.DeleteQueueAsync(Client.Path));
        }

        public async Task<BrokeredMessage> BlockingReceiveAsync()
        {
            while (true)
            {
                var message =
                    await RetryPolicy.ExecuteAsync(async () => await Client.ReceiveAsync(TimeSpan.FromMinutes(60)));
                if (message != null) return message;
            }
        }

        public async Task SetLockDurationAsync(TimeSpan durationTimeSpan)
        {
            var queueDescription = await GetQueueDescriptionAsync();
            queueDescription.LockDuration = durationTimeSpan;
            await SetQueueDescriptionAsync(queueDescription);
        }

        public void OnMessageAsync(Func<BrokeredMessage, Task> asyncAction, OnMessageOptions onMessageOptions)
        {
            Client.OnMessageAsync(asyncAction, onMessageOptions);
        }

        public async Task ActivateAsync()
        {
            var queueDescription = await GetQueueDescriptionAsync();
            queueDescription.Status = EntityStatus.Active;
            await SetQueueDescriptionAsync(queueDescription);
        }

        public async Task DisableAsync()
        {
            var queueDescription = await GetQueueDescriptionAsync();
            queueDescription.Status = EntityStatus.ReceiveDisabled;
            await SetQueueDescriptionAsync(queueDescription);
        }

        public async Task SafeAbandonAsync(BrokeredMessage message)
        {
            try
            {
                if (_receiveMode == ReceiveMode.ReceiveAndDelete)
                {
                    Log.Warning(
                        "Not expected to abandon messages on the subscription \"{0}\" that has ReceiveMode={1}.",
                        this, _receiveMode);
                    return;
                }
                await RetryPolicy.ExecuteAsync(async () => await message.AbandonAsync());
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                // It does not matter if we fail to abandon.
                // Most probably, that only means that it already has been abandoned.
            }
        }

        public async Task SafeCompleteAsync<T>(BrokeredMessage message, T interpretedMessage)
        {
            try
            {
                if (_receiveMode == ReceiveMode.ReceiveAndDelete)
                {
                    return;
                }
                await RetryPolicy.ExecuteAsync(async () => await message.CompleteAsync());
            }
            catch (MessageLockLostException ex)
            {
                Log.Error(ex, "Queue {1}: {0} could not be completed (due to lock lost)", interpretedMessage, Name);
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Queue {1}: {0} could not be completed.", interpretedMessage, Name);
            }
        }

        public bool IsPeekReceiveMode { get { return _receiveMode == ReceiveMode.PeekLock; } }

        public async Task SafeDeadLetterAsync(BrokeredMessage message)
        {
            try
            {
                await RetryPolicy.ExecuteAsync(async () => await message.DeadLetterAsync());
            }
            catch (MessageLockLostException ex)
            {
                Log.Error(ex, "Queue {0}: Message could not be put on the dead letter queue (due to lock lost)", Name);
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Queue {0}: Message could not be put on the dead letter queue", Name);
            }
        }

        public async Task<BrokeredMessage> NonBlockingReceiveAsync()
        {
            return await RetryPolicy.ExecuteAsync(async () => await Client.ReceiveAsync(TimeSpan.FromSeconds(1)));
        }

        public string Name { get; private set; }

        public async Task SendAsync<T>(T message, IDictionary<string, object> properties = null)
        {
            var m = SetProperties(message, properties);
            await SendAsync(m);
        }

        public async Task ResendAndCompleteAsync(BrokeredMessage message, IQueueReceiver queueReceiver)
        {
            var newMessage = message.Clone();
            await SendAsync(newMessage);

            if (!queueReceiver.IsPeekReceiveMode)
            {
                return;
            }

            try
            {
                await RetryPolicy.ExecuteAsync(message.CompleteAsync);
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        public async Task<long> GetLengthAsync()
        {
            var queueDescription = await GetQueueDescriptionAsync();
            return queueDescription.MessageCountDetails.ActiveMessageCount;
        }

        public long GetLength()
        {
            var task = GetLengthAsync();
            task.Wait();
            return task.Result;
        }

        public async Task<bool> IsEmptyAsync()
        {
            return await GetLengthAsync() == 0;
        }

        public bool IsEmpty()
        {
            var task = IsEmptyAsync();
            task.Wait();
            return task.Result;
        }

        public async Task FlushAsync()
        {

            await ForEachMessageAsyncUsingReceiveAndDeleteMode(async message => await Task.Run(() => { }));


            do
            {
                var deadLetterPath = QueueClient.FormatDeadLetterPath(Name);
                var deadLetterClient = QueueClient.CreateFromConnectionString(ConnectionString, deadLetterPath,
                    ReceiveMode.ReceiveAndDelete);

                var messages = await deadLetterClient.ReceiveBatchAsync(100, TimeSpan.FromMilliseconds(1000));
                var brokeredMessages = messages as BrokeredMessage[] ?? messages.ToArray();
                if (!brokeredMessages.Any()) break;
            } while (true);
        }

        public async Task ForEachMessageAsyncUsingReceiveAndDeleteMode(Func<BrokeredMessage, Task> actionAsync)
        {
            do
            {
                var client = QueueClient.CreateFromConnectionString(ConnectionString, Name,
                    ReceiveMode.ReceiveAndDelete);
                var messages = await client.ReceiveBatchAsync(100, TimeSpan.FromMilliseconds(1000));
                var brokeredMessages = messages as BrokeredMessage[] ?? messages.ToArray();
                if (!brokeredMessages.Any()) break;

                Parallel.ForEach(brokeredMessages, async brokeredMessage => { await actionAsync(brokeredMessage); });
            } while (true);
        }

        public Task CloseAsync()
        {
            return Client.CloseAsync();
        }

        public void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions)
        {
            Client.OnMessage(action, onMessageOptions);
        }

        private static BrokeredMessage SetProperties<T>(T message, IDictionary<string, object> properties)
        {
            var m = new BrokeredMessage(message, new DataContractSerializer(typeof (T)));
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    m.Properties.Add(property);
                }
            }
            return m;
        }

        private async Task SafeCreateQueueAsync(string name)
        {
            if (await NamespaceManager.QueueExistsAsync(name)) return;

            try
            {
                // Configure Queue Settings
                var qd = new QueueDescription(name);
                await RetryPolicy.ExecuteAsync(async () => await NamespaceManager.CreateQueueAsync(qd));
            }
            catch (Exception)
            {
                if (NamespaceManager.QueueExists(name)) return;
                throw;
            }
        }

        public T GetFromQueue<T>(out BrokeredMessage message) where T : class
        {
            var task = BlockingReceiveAsync();
            task.Wait();
            message = task.Result;

            return message.GetBody<T>(new DataContractSerializer(typeof (T)));
        }

        public async Task SendAsync(BrokeredMessage message)
        {
            await RetryPolicy.ExecuteAsync(() => Client.SendAsync(message));
        }

        private async Task<QueueDescription> GetQueueDescriptionAsync()
        {
            return await RetryPolicy.ExecuteAsync(async () => await NamespaceManager.GetQueueAsync(Client.Path));
        }

        private async Task SetQueueDescriptionAsync(QueueDescription queueDescription)
        {
            await RetryPolicy.ExecuteAsync(async () => await NamespaceManager.UpdateQueueAsync(queueDescription));
        }
    }
}