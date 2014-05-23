using System;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Subscription : IQueueReceiver
    {
        private readonly Topic _topic;
        public string Name { get { return Client.Name; } }
        public Subscription(Topic topic, string name, Filter filter)
        {
            _topic = topic;
            topic.GetOrCreateSubscription(name, filter);
            Client = SubscriptionClient.CreateFromConnectionString(topic.ConnectionString, topic.Client.Path, name);
        }

        public Topic Topic { get { return _topic; } }

        public SubscriptionClient Client { get; private set; }

        public T GetOneMessage<T>(out BrokeredMessage message) where T : class
        {
            do
            {
                message = NonBlockingReceive();
            } while (message == null);

            return message.GetBody<T>();
        }

        public BrokeredMessage NonBlockingReceive()
        {
            return Client.Receive(TimeSpan.FromSeconds(1));
        }

        public BrokeredMessage BlockingReceive()
        {
            while (true)
            {
                var message = Client.Receive(TimeSpan.FromMinutes(60));
                if (message != null) return message;
            }
        }

        public void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions)
        {
            Client.OnMessage(action, onMessageOptions);
        }

        public void Activate()
        {
            var subscriptionDescription = _topic.NamespaceManager.GetSubscription(Client.TopicPath, Name);
            subscriptionDescription.Status = EntityStatus.Active;
            _topic.NamespaceManager.UpdateSubscription(subscriptionDescription);
        }

        public void Disable()
        {
            var subscriptionDescription = _topic.NamespaceManager.GetSubscription(Client.TopicPath, Name);
            subscriptionDescription.Status = EntityStatus.ReceiveDisabled;
            _topic.NamespaceManager.UpdateSubscription(subscriptionDescription);
        }

        public long GetLength()
        {
            return _topic.NamespaceManager.GetSubscription(Client.TopicPath, Name).MessageCount;
        }
    }
}
