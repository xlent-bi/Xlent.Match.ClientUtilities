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

        public void Delete()
        {
            RetryPolicy.ExecuteAction(() => NamespaceManager.DeleteTopic(Client.Path));
        }

        public async Task DeleteAsync()
        {
            await RetryPolicy.ExecuteAsync(() => NamespaceManager.DeleteTopicAsync(Client.Path));
        }

        public string Name { get; private set; }

        public async Task ResendAndCompleteAsync(BrokeredMessage message)
        {
            await ResendAsync(message);
            try
            {
                await RetryPolicy.ExecuteAsync(message.CompleteAsync);
            }
                // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        public void Send<T>(T message, IDictionary<string, object> properties)
        {
            var m = new BrokeredMessage(message, new DataContractSerializer(typeof (T)));
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    m.Properties.Add(property);
                }
            }
            Send(m);
        }

        public void Send(BrokeredMessage message)
        {
            RetryPolicy.ExecuteAction(() => Client.Send(message));
        }

        public long GetLength()
        {
            return GetTopicDescription().MessageCountDetails.ActiveMessageCount;
        }

        public async Task FlushAsync()
        {
            await Task.Run(() => Flush());
        }

        public async Task ForEachMessageAsync(Func<BrokeredMessage, Task> actionAsync)
        {
            await ForEachSubscriptionAsync(subscription => subscription.ForEachMessageAsync(actionAsync));
        }

        private void CreateTopicTransient(string name)
        {
            if (NamespaceManager.TopicExists(name)) return;

            try
            {
                // Configure Topic Settings
                var qd = new TopicDescription(name);
                RetryPolicy.ExecuteAction(() => NamespaceManager.CreateTopic(qd));
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

        public SubscriptionClient GetOrCreateSubscription(string name, Filter filter,
            ReceiveMode receiveMode = ReceiveMode.PeekLock)
        {
            if (SubscriptionExists(name)) return CreateSubscriptionClient(name, receiveMode);

            try
            {
                if (filter == null)
                {
                    CreateSubscription(name);
                }
                else
                {
                    CreateSubscription(name, filter);
                }
                return CreateSubscriptionClient(name, receiveMode);
            }
            catch (Exception)
            {
                if (SubscriptionExists(name)) return CreateSubscriptionClient(name, receiveMode);
                throw;
            }
        }

        protected internal SubscriptionClient CreateSubscriptionClient(string name, ReceiveMode receiveMode)
        {
            return
                RetryPolicy.ExecuteAction(
                    () => MessagingFactory.CreateSubscriptionClient(Client.Path, name, receiveMode));
        }

        private SubscriptionDescription CreateSubscription(string name)
        {
            return RetryPolicy.ExecuteAction(() => NamespaceManager.CreateSubscription(Client.Path, name));
        }

        private SubscriptionDescription CreateSubscription(string name, Filter filter)
        {
            return RetryPolicy.ExecuteAction(() => NamespaceManager.CreateSubscription(Client.Path, name, filter));
        }

        private bool SubscriptionExists(string name)
        {
            return RetryPolicy.ExecuteAction(() => NamespaceManager.SubscriptionExists(Client.Path, name));
        }

        public void Flush()
        {
            ForEachSubscriptionAsync(async subscription => await subscription.FlushAsync()).Wait();
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

        private TopicDescription GetTopicDescription()
        {
            return RetryPolicy.ExecuteAction(() => NamespaceManager.GetTopic(Client.Path));
        }

        private void SetTopicDescription(TopicDescription topicDescription)
        {
            RetryPolicy.ExecuteAction(() => NamespaceManager.UpdateTopic(topicDescription));
        }
    }
}