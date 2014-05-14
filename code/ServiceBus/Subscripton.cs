using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Subscription
    {
        public Subscription(Topic topic, string name, Filter filter)
        {
            topic.GetOrCreateSubscription(name, filter);
            Client = SubscriptionClient.CreateFromConnectionString(topic.ConnectionString, topic.Client.Path, name);
        }

        public SubscriptionClient Client { get; private set; }

        public BrokeredMessage GetOneMessageOrNull()
        {
            return Client.Receive();
        }

        public T GetOneMessage<T>(out BrokeredMessage message) where T : class
        {
            do
            {
                message = GetOneMessageOrNull();
            } while (message == null);

            return message.GetBody<T>();
        }
    }
}
