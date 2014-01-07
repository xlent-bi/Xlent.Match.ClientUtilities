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
            var response = new ClientUtilities.Messages.SuccessResponse(request);
            MatchObjectControl control = null;
            switch (request.RequestType)
            {
                case "Get":
                    response.Data = BusinessLogic.GetData(request.Key);
                    break;
                case "Update":
                    BusinessLogic.UpdateData(request.Key, request.Data);
                    response.Data = BusinessLogic.GetData(request.Key);
                    break;
                case "Create":
                    string matchId = request.Key.MatchId;
                    try
                    {
                        // Maybe the object already exists, then we just need to update it.
                        BusinessLogic.UpdateData(request.Key, request.Data);
                    }
                    catch (Exception)
                    {
                        // The object did not exist, we must create it
                        response.Key.Value = BusinessLogic.CreateData(request.Key, request.Data);
                    }
                    response.Data = BusinessLogic.GetData(response.Key);
                    response.Key.MatchId = matchId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("request", string.Format("Unknown request type: \"{0}\"", request.RequestType));
            }

            return response;
        }
    }
}
