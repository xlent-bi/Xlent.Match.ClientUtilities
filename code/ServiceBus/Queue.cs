using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Queue : BaseClass, IQueueSender, IQueueReceiver, IQueueAdministrator
    {
        public Queue(string connectionStringName, string name)
            : base(connectionStringName)
        {


            // Create a new Queue with custom settings
            if (!NamespaceManager.QueueExists(name))
            {
                try
                {
                    // Configure Queue Settings
                    var qd = new QueueDescription(name);
                    //qd.MaxSizeInMegabytes = 5120;
                    //qd.DefaultMessageTimeToLive = new TimeSpan(0, 1, 0);
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

        public void Enqueue<T>(T message, IDictionary<string, object> properties = null)
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

        public void Resend(BrokeredMessage message)
        {
            var newMessage = message.Clone();
            Client.Send(newMessage);
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

        public BrokeredMessage BlockingReceive()
        {
            while (true)
            {
                var message = Client.Receive(new TimeSpan(0, 60, 0));
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
    }
}
