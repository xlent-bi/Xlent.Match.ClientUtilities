using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Queue : BaseClass, IQueueSender, IQueueReceiver, IQueueAdministrator
    {
        public string Name { get; private set; }
        public Queue(string connectionStringName, string name)
            : base(connectionStringName)
        {
            Name = name;
            // Create a new Queue with custom settings
            SafeCreateQueue(name);
            Client = QueueClient.CreateFromConnectionString(ConnectionString, name);
        }

        private QueueClient Client { get; set; }

        private void SafeCreateQueue(string name)
        {
            if (NamespaceManager.QueueExists(name)) return;

            try
            {
                // Configure Queue Settings
                var qd = new QueueDescription(name);
                RetryPolicy.ExecuteAction(() => NamespaceManager.CreateQueue(qd));
            }
            catch (Exception)
            {
                if (NamespaceManager.QueueExists(name)) return;
                throw;
            }
        }

        public void Send<T>(T message, IDictionary<string, object> properties = null)
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

        public T GetFromQueue<T>(out BrokeredMessage message) where T : class
        {
            message = BlockingReceive();

            return message.GetBody<T>(new DataContractSerializer(typeof(T)));
        }

        public void Send(BrokeredMessage message)
        {
            RetryPolicy.ExecuteAction(() => Client.Send(message));
        }

        public void ResendAndComplete(BrokeredMessage message)
        {
            var newMessage = message.Clone();
            Send(newMessage);
            try
            {
                 RetryPolicy.ExecuteAction(message.Complete);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }

        public BrokeredMessage NonBlockingReceive()
        {
            return RetryPolicy.ExecuteAction(() => Client.Receive(TimeSpan.FromSeconds(1)));
        }

        public async Task<BrokeredMessage> NonBlockingReceiveAsync()
        {
            return await RetryPolicy.ExecuteAction(() => Client.ReceiveAsync(TimeSpan.FromSeconds(1)));
        }

        public BrokeredMessage BlockingReceive()
        {
            while (true)
            {
                BrokeredMessage message = null;
                RetryPolicy.ExecuteAction(() => message = Client.Receive(TimeSpan.FromMinutes(60)));
                if (message != null) return message;
            }
        }

        public long GetLength()
        {
            var queueDescription = GetQueueDescription();
            return queueDescription.MessageCountDetails.ActiveMessageCount;
        }

        public void Delete()
        {
            RetryPolicy.ExecuteAction(() => NamespaceManager.DeleteQueue(Client.Path));
        }

        public async Task DeleteAsync()
        {
            await RetryPolicy.ExecuteAsync(() => NamespaceManager.DeleteQueueAsync(Client.Path));
        }

        public async Task FlushAsync()
        {
            do
            {
                var task = NonBlockingReceiveAsync();
                var message = await task;
                if (message == null) break;
                await message.CompleteAsync();
            } while (true);
        }


        public void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions)
        {
            Client.OnMessage(action, onMessageOptions);
        }

        public void Activate()
        {
            var queueDescription = GetQueueDescription();
            queueDescription.Status = EntityStatus.Active;
            SetQueueDescription(queueDescription);
        }

        public void Disable()
        {
            var queueDescription = GetQueueDescription();
            queueDescription.Status = EntityStatus.ReceiveDisabled;
            SetQueueDescription(queueDescription);
        }

        private QueueDescription GetQueueDescription()
        {
            return RetryPolicy.ExecuteAction(() => NamespaceManager.GetQueue(Client.Path));
        }

        private void SetQueueDescription(QueueDescription queueDescription)
        {
            RetryPolicy.ExecuteAction(() => NamespaceManager.UpdateQueue(queueDescription));
        }
    }
}
