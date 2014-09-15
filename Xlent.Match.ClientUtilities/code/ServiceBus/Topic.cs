using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Topic : BaseClass, IQueueSender, IQueueAdministrator
    {
        public Topic(string connectionStringName, string name)
            : base(connectionStringName)
        {
            Name = name;
            CreateTopicTransient(name);

            Client = MessagingFactory.CreateTopicClient(name);
        }

        public Topic(string connectionStringName, string pairedConnectionStringName, string name)
            : base(connectionStringName)
        {
            Name = name;

            CreatePairedNamespaceManager(pairedConnectionStringName);
            CreateTopicTransient(name);

            Client = MessagingFactory.CreateTopicClient(name);
        }

        private TopicClient Client { get; set; }

        public IEnumerable<SubscriptionDescription> Subscriptions
        {
            get { return NamespaceManager.GetSubscriptions(Client.Path); }
        }

        public async Task DeleteAsync()
        {
            await RetryPolicy.ExecuteAsync(() => NamespaceManager.DeleteTopicAsync(Client.Path));
        }

        public string Name { get; private set; }

        public async Task ResendAndCompleteAsync(BrokeredMessage message, IQueueReceiver queueReceiver)
        {
            await ResendAsync(message);
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

        public async Task SendAsync<T>(T message, IDictionary<string, object> properties)
        {
            var m = SetProperties(message, properties);
            await SendAsync(m);
        }

        public async Task<long> GetLengthAsync()
        {
            var topicDescription = await GetTopicDescriptionAsync();
            return topicDescription.MessageCountDetails.ActiveMessageCount;
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
            await ForEachSubscriptionAsync(async subscription => await subscription.FlushAsync());
        }

        public async Task ForEachMessageAsyncUsingReceiveAndDeleteMode(Func<BrokeredMessage, Task> actionAsync)
        {
            await ForEachSubscriptionAsync(subscription => subscription.ForEachMessageAsyncUsingReceiveAndDeleteMode(actionAsync));
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

        private void CreateTopicTransient(string name)
        {
            if (NamespaceManager.TopicExists(name)) return;

            try
            {
                // Configure Topic Settings
                var qd = new TopicDescription(name);
                RetryPolicy.ExecuteAsync(async () => await NamespaceManager.CreateTopicAsync(qd));
            }
            catch (Exception)
            {
                if (NamespaceManager.TopicExists(name)) return;
                throw;
            }
        }

        public async Task ResendAsync(BrokeredMessage message)
        {
            var newMessage = message.Clone();
            await SendAsync(newMessage);
        }

        public async Task SendAsync(BrokeredMessage message)
        {
            await RetryPolicy.ExecuteAsync(() => Client.SendAsync(message));
        }

        public async Task<SubscriptionClient> GetOrCreateSubscriptionAsync(string name, Filter filter,
            ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            if (await SubscriptionExistsAsync(name)) return CreateSubscriptionClient(name, receiveMode);

            try
            {
                if (filter == null)
                {
                    await CreateSubscriptionAsync(name);
                }
                else
                {
                    await CreateSubscriptionAsync(name, filter);
                }
                return CreateSubscriptionClient(name, receiveMode);
            }
            catch (Exception)
            {
                var task = SubscriptionExistsAsync(name);
                task.Wait();
                var subscriptionExists = task.Result;
                if (subscriptionExists) return CreateSubscriptionClient(name, receiveMode);
                throw;
            }
        }

        protected internal SubscriptionClient CreateSubscriptionClient(string name, ReceiveMode receiveMode)
        {
            return
                RetryPolicy.ExecuteAction(
                    () => MessagingFactory.CreateSubscriptionClient(Client.Path, name, receiveMode));
        }

        private async Task<SubscriptionDescription> CreateSubscriptionAsync(string name)
        {
            return
                await
                    RetryPolicy.ExecuteAsync(
                        async () => await NamespaceManager.CreateSubscriptionAsync(Client.Path, name));
        }

        private async Task<SubscriptionDescription> CreateSubscriptionAsync(string name, Filter filter)
        {
            return
                await
                    RetryPolicy.ExecuteAsync(
                        async () => await NamespaceManager.CreateSubscriptionAsync(Client.Path, name, filter));
        }

        private async Task<bool> SubscriptionExistsAsync(string name)
        {
            return
                await
                    RetryPolicy.ExecuteAsync(
                        async () => await NamespaceManager.SubscriptionExistsAsync(Client.Path, name));
        }

        public async Task ForEachSubscriptionAsync(Func<Subscription, Task> subscriptionActionAsync)
        {
            await Task.WhenAll(Subscriptions
                .Select(subscriptionDescription => new Subscription(this, subscriptionDescription))
                .Select(subscriptionActionAsync));
        }

        public void ForEachSubscription(Action<Subscription> subscriptionAction)
        {
            ForEachSubscriptionAsync(async subscription => await Task.Run(() => subscriptionAction(subscription)))
                .Wait();
        }

        private async Task<TopicDescription> GetTopicDescriptionAsync()
        {
            return await RetryPolicy.ExecuteAsync(async () => await NamespaceManager.GetTopicAsync(Client.Path));
        }

        private async Task SetTopicDescriptionAsync(TopicDescription topicDescription)
        {
            await RetryPolicy.ExecuteAsync(async () => await NamespaceManager.UpdateTopicAsync(topicDescription));
        }
    }
}