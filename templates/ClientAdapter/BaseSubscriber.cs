using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading;
using Xlent.Match.ClientUtilities;

namespace ClientAdapter
{
    public class BaseSubscriber
    {
        /// <summary>
        /// This is the eternal loop that will receive all requests and call <see cref="HandleRequest"/> for each.
        /// </summary>
        public static void HandleRequests(string entityName, AdapterSubscription.RequestDelegate requestHandler)
        {
            AdapterSubscription subscription = null;
            var sleepTime = 100;

            while (true)
            {
                try
                {
                    if (subscription == null)
                    {
                        subscription = new AdapterSubscription("Crm", entityName);
                    }

                    // Handle one request
                    subscription.HandleOne(requestHandler);
                    // Successful receive. Reset waiting time.
                    sleepTime = 100;
                }
                catch (Exception ex)
                {
                    // There was a problem with receiving a request, wait some time before we try again.
                    Thread.Sleep(sleepTime);
                    // Next time wait twice as long
                    sleepTime = sleepTime * 2;
                    if (ex is MessagingEntityNotFoundException)
                    {
                        // The topic or subscription has disappeared, create a new one
                        subscription = null;
                    }
                }
            }
        }
    }
}
