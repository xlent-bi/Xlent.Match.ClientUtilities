using System;
using System.Runtime.Serialization;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
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
            Client = topic.GetOrCreateSubscription(name, filter);
       }

        public Topic Topic { get { return _topic; } }

        public RetryPolicy<ServiceBusTransientErrorDetectionStrategy> RetryPolicy
        {
            get { return _topic.RetryPolicy; }
        }

        private SubscriptionClient Client { get; set; }

        public T GetOneMessage<T>(out BrokeredMessage message) where T : class
        {
            do
            {
                message = BlockingReceive();
            } while (message == null);

            return message.GetBody<T>(new DataContractSerializer(typeof(T)));
        }

        public BrokeredMessage NonBlockingReceive()
        {
            return RetryPolicy.ExecuteAction(() => Client.Receive(TimeSpan.FromSeconds(1)));
        }

        public BrokeredMessage BlockingReceive()
        {
            while (true)
            {
                //var message = RetryPolicy.ExecuteAction(() => Client.Receive(TimeSpan.FromMinutes(60)));
                var message = Client.Receive(TimeSpan.FromMinutes(60));
                if (message != null) return message;
            }
        }

        public void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions)
        {
            Client.OnMessage(action, onMessageOptions);
        }

        public void Close()
        {
            RetryPolicy.ExecuteAction(() => Client.Close());
        }

        public void Activate()
        {
            var subscriptionDescription = GetSubscriptionDescription();
            subscriptionDescription.Status = EntityStatus.Active;
            SetSubscriptionDescription(subscriptionDescription);
        }
         
        private void SetSubscriptionDescription(SubscriptionDescription subscriptionDescription)
        {
            RetryPolicy.ExecuteAction(() => _topic.NamespaceManager.UpdateSubscription(subscriptionDescription));
        }

        private SubscriptionDescription GetSubscriptionDescription()
        {
            return RetryPolicy.ExecuteAction(() => _topic.NamespaceManager.GetSubscription(Client.TopicPath, Name));
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
    }
}
