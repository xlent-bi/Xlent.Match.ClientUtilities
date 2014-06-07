using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
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
            if (!NamespaceManager.TopicExists(name))
            {
                try
                {
                    // Configure Topic Settings
                    var td = new TopicDescription(name);
                    //qd.MaxSizeInMegabytes = 5120;
                    //qd.DefaultMessageTimeToLive = TimeSpan.FromSeconds(60);
                    NamespaceManager.CreateTopic(td);
                }
                catch (Exception)
                {
                    if (!NamespaceManager.TopicExists(name))
                    {
                        throw;
                    }
                    // Somebody else beat us and created the queue.
                }
            }

            Client = TopicClient.CreateFromConnectionString(ConnectionString, name);
        }

        public TopicClient Client { get; private set; }

        public void Resend(BrokeredMessage message)
        {
            var newMessage = message.Clone();
            Send(newMessage);
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

        public void GetOrCreateSubscription(string name, Filter filter)
        {
            if (NamespaceManager.SubscriptionExists(Client.Path, name)) return;

            try
            {
                if (filter == null)
                {
                    NamespaceManager.CreateSubscription(Client.Path, name);
                }
                else
                {
                    NamespaceManager.CreateSubscription(Client.Path, name, filter);
                }
            }
            catch (Exception)
            {
                if (!NamespaceManager.SubscriptionExists(Client.Path, name))
                {
                    throw;
                }
                // Somebody else beat us and created the queue.
            }
        }

        public long GetLength()
        {
            return NamespaceManager.GetTopic(Client.Path).MessageCountDetails.ActiveMessageCount;
        }

        public void Delete()
        {
            NamespaceManager.DeleteTopic(Client.Path);
        }

        public async Task DeleteAsync()
        {
            await NamespaceManager.DeleteTopicAsync(Client.Path);
        }

        public void Send(BrokeredMessage message)
        {
            Client.Send(message);
        }

        public void ResendAndComplete(BrokeredMessage message)
        {
            var newMessage = message.Clone();
            Client.Send(newMessage);
            try
            {
                message.Complete();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        public string Name { get; private set; }
    }
}
