using Microsoft.ServiceBus.Messaging;
using System;
using System.Threading;
using Xlent.Match.ClientUtilities;

namespace Xlent.Match.Test.ClientAdapter
{
    class Subscriber
    {
        private static readonly BusinessLogic BusinessLogic = new BusinessLogic();

        /// <summary>
        /// This is the eternal loop that will receive all requests and call <see cref="HandleRequest"/> for each.
        /// </summary>
        public static void HandleRequests()
        {
            AdapterSubscription subscription = null;
            var sleepTime = 100;

            while (true)
            {
                try
                {
                    if (subscription == null)
                    {
                        subscription = new AdapterSubscription();
                    }

                    // Handle one request
                    subscription.HandleOne(HandleRequest);
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

        /// <summary>
        /// Handle one request.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <returns>A successful response.</returns>
        /// <remarks>If the method fails, it will throw an exception, corresponding to the failure types
        /// in http://xlentmatch.com/wiki/FailureResponse_Message#Error_Types</remarks>
        private static ClientUtilities.Messages.SuccessResponse HandleRequest(ClientUtilities.Messages.Request request)
        {
            // Create a basic SuccessResponse message, see http://xlentmatch.com/wiki/SuccessResponse_Message
            var response = new ClientUtilities.Messages.SuccessResponse(request);

            // Check which object this request is for
            var mainKey = request.MatchObject.Key;

            MatchObjectControl control = null;
            switch (request.RequestType)
            {
                case "Get":
                    response.MatchObject = BusinessLogic.GetObject(mainKey);
                    mainKey = response.MatchObject.Key;
                    break;
                case "Update":
                    BusinessLogic.UpdateObject(request);
                    control = new MatchObjectControl(mainKey.ClientName, mainKey.EntityName, mainKey.Value);
                    response.MatchObject = control.MatchObject;
                    break;
                case "Create":
                    try
                    {
                        // Maybe the object already exists, then we just need to update it.
                        BusinessLogic.UpdateObject(request);
                    }
                    catch (Exception)
                    {
                        // The object did not exist, we must create it
                        mainKey.Value = BusinessLogic.CreateObject(request);
                    }
                    control = new MatchObjectControl(mainKey.ClientName, mainKey.EntityName, mainKey.Value, mainKey.MatchId);
                    response.MatchObject = control.MatchObject;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("request", string.Format("Unknown request type: \"{0}\"", request.RequestType));
            }

            return response;
        }
    }
}
