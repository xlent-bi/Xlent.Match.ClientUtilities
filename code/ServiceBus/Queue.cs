using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;

namespace Xlent.Match.ClientUtilities.ServiceBus
{
    public class Queue : BaseClass
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
            Client.Send(m);
        }

        public T GetFromQueue<T>(out BrokeredMessage message) where T : class
        {
            do
            {
                message = Client.Receive();
            } while (message == null);

            return message.GetBody<T>();
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
