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
            Client = topic.GetOrCreateSubscription(name, filter, receiveMode);
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

        public BrokeredMessage NonBlockingReceive()
        {
            return RetryPolicy.ExecuteAction(() => Client.Receive(TimeSpan.FromSeconds(1)));
        }

        public BrokeredMessage BlockingReceive()
        {
            while (true)
            {
                var message = RetryPolicy.ExecuteAction(() => Client.Receive(TimeSpan.FromMinutes(60)));
                if (message != null) return message;
            }
        }

        public void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions)
        {
            Client.OnMessage(action, onMessageOptions);
        }

        public void OnMessageAsync(Func<BrokeredMessage, Task> asyncAction, OnMessageOptions onMessageOptions)
        {
            Client.OnMessageAsync(asyncAction, onMessageOptions);
        }

        public void Activate()
        {
            var subscriptionDescription = GetSubscriptionDescription();
            subscriptionDescription.Status = EntityStatus.Active;
            SetSubscriptionDescription(subscriptionDescription);
        }

        public void SetLockDuration(TimeSpan durationTimeSpan)
        {
            var description = GetSubscriptionDescription();
            description.LockDuration = durationTimeSpan;
            SetSubscriptionDescription(description);
        }

        public void Disable()
        {
            var subscriptionDescription = GetSubscriptionDescription();
            subscriptionDescription.Status = EntityStatus.ReceiveDisabled;
            SetSubscriptionDescription(subscriptionDescription);
        }

        public long GetLength()
        {
            return GetSubscriptionDescription().MessageCountDetails.ActiveMessageCount;
        }

        public async Task FlushAsync()
        {
            await ForEachMessageAsync(async message => await Task.Run(() => { }));
            var length = GetLength();
            //Debug.Assert(length == 0, "Expected subscription to be empty after flush", "Subscription \"{0}\" = {1}", Name, length);

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

        public async Task ForEachMessageAsync(Func<BrokeredMessage, Task> actionAsync)
        {
            do
            {
                var flushClient = SubscriptionClient.CreateFromConnectionString(_topic.ConnectionString, _topic.Name,
                    Name, ReceiveMode.ReceiveAndDelete);
                var messages = await flushClient.ReceiveBatchAsync(100, TimeSpan.FromMilliseconds(1000));
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
                await RetryPolicy.ExecuteAction(() => message.AbandonAsync());
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
                await RetryPolicy.ExecuteAction(() => message.CompleteAsync());
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
                await RetryPolicy.ExecuteAction(() => message.DeadLetterAsync());
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

        public T GetOneMessage<T>(out BrokeredMessage message) where T : class
        {
            do
            {
                message = BlockingReceive();
            } while (message == null);

            return message.GetBody<T>(new DataContractSerializer(typeof (T)));
        }

        public T GetOneMessageNoBlocking<T>(out BrokeredMessage message) where T : class
        {
            message = NonBlockingReceive();
            return message == null ? null : message.GetBody<T>(new DataContractSerializer(typeof (T)));
        }

        public async Task<BrokeredMessage> NonBlockingReceiveAsync()
        {
            return await RetryPolicy.ExecuteAction(() => Client.ReceiveAsync(TimeSpan.FromSeconds(1)));
        }

        public void Close()
        {
            RetryPolicy.ExecuteAction(() => Client.Close());
        }

        private void SetSubscriptionDescription(SubscriptionDescription subscriptionDescription)
        {
            RetryPolicy.ExecuteAction(() => _topic.NamespaceManager.UpdateSubscription(subscriptionDescription));
        }

        private SubscriptionDescription GetSubscriptionDescription()
        {
            return RetryPolicy.ExecuteAction(() => _topic.NamespaceManager.GetSubscription(Client.TopicPath, Name));
        }

        public override string ToString()
        {
            return String.Format("{0}/{1}", _topic.Name, Name);
        }
    }
}