using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

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

            this.Client = QueueClient.CreateFromConnectionString(ConnectionString, name);
        }

        public QueueClient Client { get; private set; }

        public void Enqueue<T>(T message, IDictionary<string, object> properties = null)
        {

            var dataContractSerializer =
                new DataContractSerializer(typeof(T));

            var m = new BrokeredMessage(message, dataContractSerializer);
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

            var dataContractSerializer =
                new DataContractSerializer(typeof(T));

            return message.GetBody<T>(dataContractSerializer);
        }

        public long GetLength()
        {
            return NamespaceManager.GetQueue(Client.Path).MessageCount;
        }

        public void Delete()
        {
            NamespaceManager.DeleteQueue(Client.Path);
        }
    }
}
