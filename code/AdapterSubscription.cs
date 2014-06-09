using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using Xlent.Match.ClientUtilities.Exceptions;
using Xlent.Match.ClientUtilities.Logging;
using Xlent.Match.ClientUtilities.MatchObjectModel;
using Xlent.Match.ClientUtilities.Messages;
using Xlent.Match.ClientUtilities.ServiceBus;

namespace Xlent.Match.ClientUtilities
{
    public class AdapterSubscription : Subscription
    {
        private static readonly Topic RequestTopic;
        private static readonly Topic ResponseTopic;

        static AdapterSubscription()
        {
            // The maximum number of concurrent connections allowed by a ServicePoint object. The default value is 2.
            ServicePointManager.DefaultConnectionLimit = 12;

            RequestTopic = new Topic("Xlent.Match.ClientUtilities.ConnectionString", "Request");
            ResponseTopic = new Topic("Xlent.Match.ClientUtilities.ConnectionString", "Response");
        }

        private AdapterSubscription(string name, Filter filter)
            : base(RequestTopic, name, filter)
        {
        }

        public AdapterSubscription()
            : this("AllClients", (Filter)null)
        {
        }

        public AdapterSubscription(string clientName)
            : this(clientName, GetFilter(clientName))
        {
        }

        public AdapterSubscription(string clientName, string entityName)
            : this(string.Join(".", clientName, entityName), GetFilter(clientName, entityName))
        {
        }

        private static Filter GetFilter(string clientName)
        {
            return new SqlFilter(string.Format("ClientName = '{0}'", clientName));
        }

        private static Filter GetFilter(string clientName, string entityName)
        {
            return new SqlFilter(string.Format("ClientName = '{0}' AND EntityName = '{1}'", clientName, entityName));
        }

        public delegate Data GetRequestDelegate(Key key);

        public delegate void UpdateRequestDelegate(Key key, Data data);

        public delegate Key CreateRequestDelegate(Key key, Data data);

        public void ProcessRequests(GetRequestDelegate getRequestDelegate,
            UpdateRequestDelegate updateRequestDelegate,
            CreateRequestDelegate createRequestDelegate,
            ManualResetEvent stopEvent, int maxConcurrentCalls = 1)
        {
            var options = new OnMessageOptions { AutoComplete = false, MaxConcurrentCalls = maxConcurrentCalls };

            OnMessage(message =>
            {
                var request = message.GetBody<Request>(new DataContractSerializer(typeof(Request)));

                SafeProcessRequest(getRequestDelegate, updateRequestDelegate, createRequestDelegate, request, message);

            }, options);

            stopEvent.WaitOne();
            Close();
        }

        /// <summary>
        /// For test purposes!
        /// </summary>
        public void ProcessOneMessage(GetRequestDelegate getRequestDelegate,
            UpdateRequestDelegate updateRequestDelegate,
            CreateRequestDelegate createRequestDelegate)
        {
            BrokeredMessage message;
            var request = GetOneMessage<Request>(out message);
            if (request == null) return;

            SafeProcessRequest(getRequestDelegate, updateRequestDelegate, createRequestDelegate, request, message);
        }

        public static SuccessResponse ProcessRequest(GetRequestDelegate getRequestDelegate,
            UpdateRequestDelegate updateRequestDelegate,
            CreateRequestDelegate createRequestDelegate, Request request)
        {
            try
            {
                var response = new SuccessResponse(request);

                switch (request.RequestType)
                {
                    case Request.RequestTypeEnum.Get:
                        response.Data = getRequestDelegate(request.Key);
                        break;
                    case Request.RequestTypeEnum.Update:
                        updateRequestDelegate(request.Key, request.Data);
                        break;
                    case Request.RequestTypeEnum.Create:
                        var matchId = request.Key.MatchId;
                        response.Key = createRequestDelegate(request.Key, request.Data);
                        response.Key.MatchId = matchId;
                        break;
                    default:
                        throw new BadRequestException(String.Format("Unknown request type: \"{0}\"", request.RequestType));
                }

                return response;
            }
            catch (Exception e)
            {
                if (e is MatchException) throw;

                throw new InternalServerErrorException(e);
            }
        }

        private static void SafeProcessRequest(GetRequestDelegate getRequestDelegate,
            UpdateRequestDelegate updateRequestDelegate,
            CreateRequestDelegate createRequestDelegate, Request request, BrokeredMessage message)
        {
            try
            {
                Log.Information("Processing {0} message", request.RequestTypeAsString);

                var successResponse = ProcessRequest(
                    getRequestDelegate, updateRequestDelegate, createRequestDelegate,
                    request);
                SendResponse(successResponse);
            }
            catch (MovedException exception)
            {
                Log.Information(exception.Message);

                var oldId = request.KeyValue;
                var failureResponse = new FailureResponse(request, exception.ErrorType)
                {
                    Value = oldId,
                    Message = exception.Message,
                    Key = { Value = exception.NewKeyValue }
                };

                SendResponse(failureResponse);
            }
            catch (InternalServerErrorException exception)
            {
                Log.Critical(exception, "An internal server error has occured.");

                var failureResponse = new FailureResponse(request, FailureResponse.ErrorTypeEnum.InternalServerError)
                {
                    Message = exception.ToString()
                };

                SendResponse(failureResponse);
            }
            catch (MatchException exception)
            {
                Log.Error(exception, "An error has occured");

                var failureResponse = new FailureResponse(request, exception.ErrorType)
                {
                    Message = exception.Message
                };

                SendResponse(failureResponse);
            }
            catch (Exception exception)
            {
                Log.Critical(exception, "An error not handled by the adapter has occured");

                var failureResponse = new FailureResponse(request, FailureResponse.ErrorTypeEnum.AdapterDidNotHandleException)
                {
                    Message = exception.ToString()
                };

                SendResponse(failureResponse);
            }

            // Try to complete this message since we should have sent a response, either success or failure at
            // this point.
            try
            {
                message.Complete();
            }
// ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                // Intientional error suppression
                // If we fail it could be because the message has timed out and then we will process it
                // again so we just fail silently here
            }
        }

        private static void SendResponse<T>(T response) where T : Response
        {
            ResponseTopic.Send(response, new Dictionary<string, object> { { "ResponseType", response.ResponseTypeAsString } });
        }
    }
}
