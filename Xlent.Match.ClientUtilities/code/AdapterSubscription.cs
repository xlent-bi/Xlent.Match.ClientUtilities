﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
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
        public delegate Key CreateRequestDelegate(Key key, Data data);

        public delegate Data GetRequestDelegate(Key key);

        public delegate void UpdateRequestDelegate(Key key, Data data);

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
            : base(RequestTopic, name, filter, ReceiveMode.ReceiveAndDelete)
        {
        }

        public AdapterSubscription()
            : this("AllClients", (Filter) null)
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

        public void ProcessRequests(GetRequestDelegate getRequestDelegate,
            UpdateRequestDelegate updateRequestDelegate,
            CreateRequestDelegate createRequestDelegate,
            ManualResetEvent stopEvent, int maxConcurrentCalls = 1)
        {
            var options = new OnMessageOptions {AutoComplete = false, MaxConcurrentCalls = maxConcurrentCalls};

            OnMessageAsync(async message =>
            {
                var request = message.GetBody<Request>(new DataContractSerializer(typeof (Request)));

                await
                    SafeProcessRequestAsync(getRequestDelegate, updateRequestDelegate, createRequestDelegate, request,
                        message);
            }, options);

            stopEvent.WaitOne();
            CloseAsync().Wait();
        }

        /// <summary>
        ///     For test purposes!
        /// </summary>
        public async Task<Response> ProcessOneMessageAsync(
            GetRequestDelegate getRequestDelegate,
            UpdateRequestDelegate updateRequestDelegate,
            CreateRequestDelegate createRequestDelegate,
            Request.RequestTypeEnum? requestType = null,
            string clientName = null, string entityName = null, string keyValue = null)
        {
            var expectedRequestMessage = String.Format(
                "Expected request of type {0} ({1}/{2}/{3})",
                requestType, clientName, entityName, keyValue);

            BrokeredMessage message;
            var request = GetOneMessageNoBlocking<Request>(out message);
            if (request == null) return null;
            var queue = new Queue<Request>();

            while (!IsExpectedRequest(request, requestType, clientName, entityName, keyValue))
            {
                Log.Verbose("Received {0} {1}", request, expectedRequestMessage);
                queue.Enqueue(request);
                SafeCompleteAsync(message, request).Wait();
                request = GetOneMessageNoBlocking<Request>(out message);
                if (request == null) break;
            }

            var tasks = queue.Select(SendRequestAsync).ToArray();
            await Task.WhenAll(tasks);

            if (request == null)
            {
                throw new ApplicationException(expectedRequestMessage);
            }

            return
                await
                    SafeProcessRequestAsync(getRequestDelegate, updateRequestDelegate, createRequestDelegate, request,
                        message);
        }

        private bool IsExpectedRequest(Request request, Request.RequestTypeEnum? requestType, string clientName,
            string entityName, string keyValue)
        {
            if (requestType != null && (requestType != request.RequestType)) return false;

            if (String.IsNullOrEmpty(clientName)) return true;
            if (String.Compare(request.ClientName, clientName, StringComparison.InvariantCultureIgnoreCase) != 0)
                return false;

            if (String.IsNullOrEmpty(entityName)) return true;
            if (String.Compare(request.EntityName, entityName, StringComparison.InvariantCultureIgnoreCase) != 0)
                return false;

            if (String.IsNullOrEmpty(keyValue)) return true;
            var reqeustKeyValue = request.Key.Value ?? request.Key.ReservationId;
            return (reqeustKeyValue == keyValue);
        }


        private async Task SendRequestAsync(Request request)
        {
            await RequestTopic.SendAsync(
                request,
                new CaseInsensitiveDictionary<object>
                {
                    {"RequestType", request.RequestTypeAsString},
                    {"ClientName", request.ClientName},
                    {"EntityName", request.EntityName}
                });
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
                        if (response.Data != null)
                        {
                            response.Data.CalculateCheckSum(request.KeyValue);
                        }
                        break;
                    case Request.RequestTypeEnum.Update:
                        var currentClientData = getRequestDelegate(request.Key);
                        if ((currentClientData != null) && !currentClientData.ShouldCheckSumBeIgnored())
                        {
                            currentClientData.CalculateCheckSum(request.KeyValue);
                            if (currentClientData.CheckSum != request.Data.CheckSum)
                            {
                                throw new ClientDataHasBeenUpdated(currentClientData);
                            }
                        }
                        updateRequestDelegate(request.Key, request.Data);
                        break;
                    case Request.RequestTypeEnum.Create:
                        var reservationId = request.Key.ReservationId;
                        response.Key = createRequestDelegate(request.Key, request.Data);
                        response.Key.ReservationId = reservationId;
                        break;
                    default:
                        throw new BadRequestException(String.Format("Unknown request type: \"{0}\"", request.RequestType));
                }

                return response;
            }
            catch (MatchException)
            {
                throw;
            }
            catch (SilentFailOnlyForTestingException)
            {
#if DEBUG
                Log.Warning("Silent fail. This exception should only occur when running test cases.");
                throw;
#else
                throw new InternalServerErrorException("Silent fail. This exception should only occur when running test cases.");
#endif
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException(e);
            }
        }

        private async Task<Response> SafeProcessRequestAsync(GetRequestDelegate getRequestDelegate,
            UpdateRequestDelegate updateRequestDelegate,
            CreateRequestDelegate createRequestDelegate, Request request, BrokeredMessage message)
        {
            Response responseSent = null;
            try
            {
                Log.Verbose("{0} processing {1}", request.ClientName, request);

                var successResponse = ProcessRequest(
                    getRequestDelegate, updateRequestDelegate, createRequestDelegate,
                    request);
                await SendResponseAsync(successResponse);
                responseSent = successResponse;
            }
            catch (MovedException exception)
            {
                Log.Information(exception.Message);

                var failureResponse = new FailureResponse(request, exception.ErrorType)
                {
                    Value = exception.NewKeyValue,
                    Message = exception.Message,
                };

                SendResponseAsync(failureResponse).Wait();
                responseSent = failureResponse;
            }
            catch (ClientDataHasBeenUpdated exception)
            {
                Log.Information(exception.Message);

                if (exception.NewClientData != null)
                {
                    exception.NewClientData.CalculateCheckSum(request.KeyValue);
                }

                var failureResponse = new FailureResponse(request, exception.ErrorType)
                {
                    Value = exception.NewCheckSum,
                    Data = exception.NewClientData,
                    Message = exception.Message,
                };

                SendResponseAsync(failureResponse).Wait();
                responseSent = failureResponse;
            }
            catch (InternalServerErrorException exception)
            {
                Log.Critical(exception, "An internal server error has occured.");

                var failureResponse = new FailureResponse(request, FailureResponse.ErrorTypeEnum.InternalServerError)
                {
                    Message = exception.ToString()
                };

                SendResponseAsync(failureResponse).Wait();
                responseSent = failureResponse;
            }
            catch (MatchException exception)
            {
                Log.Error(exception, "An error has occured");

                var failureResponse = new FailureResponse(request, exception.ErrorType)
                {
                    Message = exception.Message
                };

                SendResponseAsync(failureResponse).Wait();
                responseSent = failureResponse;
            }
            catch (SilentFailOnlyForTestingException)
            {
#if DEBUG
                Log.Warning("Silent fail. This exception should only occur when running test cases.");
#else
                Log.Critical("Silent fail. This exception should only occur when running test cases.");
#endif
            }
            catch (Exception exception)
            {
                Log.Critical(exception, "An error not handled by the adapter has occured");

                var failureResponse = new FailureResponse(request,
                    FailureResponse.ErrorTypeEnum.AdapterDidNotHandleException)
                {
                    Message = exception.ToString()
                };

                SendResponseAsync(failureResponse).Wait();
                responseSent = failureResponse;
            }

            // Try to complete this message since we should have sent a response, either success or failure at
            // this point.

            SafeCompleteAsync(message, request).Wait();

            return responseSent;
        }

        private static async Task SendResponseAsync<T>(T response) where T : Response
        {
            Log.Verbose("{0} sending response {1}", response.ClientName, response);
            await ResponseTopic.SendAsync(response,
                new CaseInsensitiveDictionary<object> {{"ResponseType", response.ResponseTypeAsString}});
        }
    }
}