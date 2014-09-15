using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;
using Xlent.Match.ClientUtilities.Logging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Subscription : IQueueReceiver
    {
        private readonly ReceiveMode _receiveMode;
        private readonly Topic _topic;

        public Subscription(Topic topic, string name, Filter filter,
            ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            _topic = topic;
            _receiveMode = receiveMode;
            var task = topic.GetOrCreateSubscriptionAsync(name, filter, receiveMode);
            task.Wait();
            Client = task.Result;
        }

        public Subscription(Topic topic, SubscriptionDescription subscriptionDescription,
            ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            _topic = topic;
            Client = topic.CreateSubscriptionClient(subscriptionDescription.Name, receiveMode);
        }

        public Topic Topic
        {
            get { return _topic; }
        }

        public RetryPolicy<ServiceBusTransientErrorDetectionStrategy> RetryPolicy
        {
            get { return _topic.RetryPolicy; }
        }

        private SubscriptionClient Client { get; set; }

        public string Name
        {
            get { return Client.Name; }
        }

		public bool IsPeekReceiveMode { get { return _receiveMode == ReceiveMode.PeekLock; } }

        public async Task<BrokeredMessage> BlockingReceiveAsync()
        {
            while (true)
            {
                var message =
                    await RetryPolicy.ExecuteAsync(async () => await Client.ReceiveAsync(TimeSpan.FromMinutes(60)));
                if (message != null) return message;
            }
        }

        public void OnMessageAsync(Func<BrokeredMessage, Task> asyncAction, OnMessageOptions onMessageOptions)
        {
            Client.OnMessageAsync(asyncAction, onMessageOptions);
        }

        public async Task ActivateAsync()
        {
            var subscriptionDescription = await GetSubscriptionDescriptionAsync();
            subscriptionDescription.Status = EntityStatus.Active;
            await SetSubscriptionDescriptionAsync(subscriptionDescription);
        }

        public async Task SetLockDurationAsync(TimeSpan durationTimeSpan)
        {
            var description = await GetSubscriptionDescriptionAsync();
            description.LockDuration = durationTimeSpan;
            await SetSubscriptionDescriptionAsync(description);
        }

        public async Task DisableAsync()
        {
            var subscriptionDescription = await GetSubscriptionDescriptionAsync();
            subscriptionDescription.Status = EntityStatus.ReceiveDisabled;
            await SetSubscriptionDescriptionAsync(subscriptionDescription);
        }

        public async Task<long> GetLengthAsync()
        {
            var subscriptionDescription = await GetSubscriptionDescriptionAsync();
            return subscriptionDescription.MessageCountDetails.ActiveMessageCount;
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
                var deadLetterPath = SubscriptionClient.FormatDeadLetterPath(_topic.Name, Name);
                var deadLetterMessagingFactory = MessagingFactory.CreateFromConnectionString(_topic.ConnectionString);
                var deadLetterClient =
                    await
                        deadLetterMessagingFactory.CreateMessageReceiverAsync(deadLetterPath,
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
                var client = SubscriptionClient.CreateFromConnectionString(_topic.ConnectionString, _topic.Name,
                    Name, ReceiveMode.ReceiveAndDelete);
                var messages = await client.ReceiveBatchAsync(100, TimeSpan.FromMilliseconds(1000));
                var brokeredMessages = messages as BrokeredMessage[] ?? messages.ToArray();
                if (!brokeredMessages.Any()) break;

                Parallel.ForEach(brokeredMessages, async brokeredMessage => { await actionAsync(brokeredMessage); });
            } while (true);
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

        public void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions)
        {
            Client.OnMessage(action, onMessageOptions);
        }

        public T GetOneMessageAsync<T>(out BrokeredMessage message) where T : class
        {
            do
            {
                var task = BlockingReceiveAsync();
                task.Wait();
                message = task.Result;
            } while (message == null);

            return message.GetBody<T>(new DataContractSerializer(typeof (T)));
        }

        public T GetOneMessageNoBlocking<T>(out BrokeredMessage message) where T : class
        {
            var task = NonBlockingReceiveAsync();
            task.Wait();
            message = task.Result;
            return message == null ? null : message.GetBody<T>(new DataContractSerializer(typeof (T)));
        }

        public async Task CloseAsync()
        {
            await RetryPolicy.ExecuteAsync(async () => await Client.CloseAsync());
        }

        private async Task SetSubscriptionDescriptionAsync(SubscriptionDescription subscriptionDescription)
        {
            await
                RetryPolicy.ExecuteAsync(
                    async () => await _topic.NamespaceManager.UpdateSubscriptionAsync(subscriptionDescription));
        }

        private async Task<SubscriptionDescription> GetSubscriptionDescriptionAsync()
        {
            return
                await
                    RetryPolicy.ExecuteAsync(
                        async () => await _topic.NamespaceManager.GetSubscriptionAsync(Client.TopicPath, Name));
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}", _topic.Name, Name);
        }
    }
}