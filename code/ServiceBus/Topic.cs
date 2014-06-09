using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
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

        public string Name { get; private set; }

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

        public void Resend(BrokeredMessage message)
        {
            var newMessage = message.Clone();
            Send(newMessage);
        }

        public void ResendAndComplete(BrokeredMessage message)
        {
            Resend(message);
            try
            {
                RetryPolicy.ExecuteAction(message.Complete);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        public void Send<T>(T message, IDictionary<string, object> properties)
        {
            var m = new BrokeredMessage(message, new DataContractSerializer(typeof(T)));
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

        public SubscriptionClient GetOrCreateSubscription(string name, Filter filter)
        {
            if (SubscriptionExists(name)) return CreateSubscriptionClient(name);

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
                return CreateSubscriptionClient(name);
            }
            catch (Exception)
            {
                if (SubscriptionExists(name)) return CreateSubscriptionClient(name);
                throw;
            }

        }

        private SubscriptionClient CreateSubscriptionClient(string name)
        {
            return RetryPolicy.ExecuteAction(() => MessagingFactory.CreateSubscriptionClient(Client.Path, name));
        }

        private SubscriptionDescription CreateSubscription(string name)
        {
            //return RetryPolicy.ExecuteAction(() => NamespaceManager.CreateSubscription(Client.Path, name));
            return NamespaceManager.CreateSubscription(Client.Path, name);
        }

        private SubscriptionDescription CreateSubscription(string name, Filter filter)
        {
            return RetryPolicy.ExecuteAction(() => NamespaceManager.CreateSubscription(Client.Path, name, filter));
        }

        private bool SubscriptionExists(string name)
        {
            return RetryPolicy.ExecuteAction(() => NamespaceManager.SubscriptionExists(Client.Path, name));
        }

        public long GetLength()
        {
            return GetTopicDescription().MessageCountDetails.ActiveMessageCount;
        }

        public void Delete()
        {
            RetryPolicy.ExecuteAction(() => NamespaceManager.DeleteTopic(Client.Path));
        }

        public async Task DeleteAsync()
        {
            await RetryPolicy.ExecuteAsync(() => NamespaceManager.DeleteTopicAsync(Client.Path));
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
