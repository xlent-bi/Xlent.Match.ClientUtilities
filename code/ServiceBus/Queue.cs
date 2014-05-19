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
            if (!NamespaceManager.QueueExists(name))
            {
                try
                {
                    // Configure Queue Settings
                    var qd = new QueueDescription(name);
                    //qd.MaxSizeInMegabytes = 5120;
                    //qd.DefaultMessageTimeToLive = TimeSpan.FromSeconds(60);
                    NamespaceManager.CreateQueue(qd);
                }
                catch (Exception)
                {
                    if (!NamespaceManager.QueueExists(name))
                    {
                        throw;
                    }
                    // Somebody else beat us and created the queue.
                }
            }

            Client = QueueClient.CreateFromConnectionString(ConnectionString, name);
        }

        public QueueClient Client { get; private set; }

        public void Send<T>(T message, IDictionary<string, object> properties = null)
        {
            var m = new BrokeredMessage(message);
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

            return message.GetBody<T>();
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

        public long GetLength()
        {
            return NamespaceManager.GetQueue(Client.Path).MessageCount;
        }

        public void Delete()
        {
            NamespaceManager.DeleteQueue(Client.Path);
        }

        public async Task DeleteAsync()
        {
            await NamespaceManager.DeleteQueueAsync(Client.Path);
    }


        public void OnMessage(Action<BrokeredMessage> action, OnMessageOptions onMessageOptions)
        {
            Client.OnMessage(action, onMessageOptions);
        }
    }
}
