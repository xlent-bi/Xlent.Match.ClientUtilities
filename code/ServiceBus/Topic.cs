using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Topic : BaseClass
    {
        public Topic(string connectionStringName, string name)
            :base(connectionStringName)
        {
            if (!NamespaceManager.TopicExists(name))
            {
                try
                {
                    // Configure Topic Settings
                    var td = new TopicDescription(name);
                    //qd.MaxSizeInMegabytes = 5120;
                    //qd.DefaultMessageTimeToLive = new TimeSpan(0, 1, 0);
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

            this.Client = TopicClient.CreateFromConnectionString(ConnectionString, name);
        }

        public TopicClient Client { get; private set; }

        public void Send<T>(T message, IDictionary<string,object> properties)
        {
            var m = new BrokeredMessage(message);
            if (properties != null)
            {
                foreach (var property in properties)
                {
                    m.Properties.Add(property);
                }
            }
            Client.Send(m);
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
    }
}
