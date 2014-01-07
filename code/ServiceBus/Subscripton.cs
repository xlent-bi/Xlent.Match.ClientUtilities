using System;
using System.Runtime.Serialization;
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

        public T GetOneMessage<T>(out BrokeredMessage message) where T : class
        {
            do
            {
                message = Client.Receive();
            } while (message == null);

            var dataContractSerializer =
                    new DataContractSerializer(typeof(T));

            return message.GetBody<T>(dataContractSerializer);
        }
    }
}
