﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
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

        public delegate SuccessResponse RequestDelegate(Request request);

        public void ProcessRequests(RequestDelegate requestDelegate, ManualResetEvent stopEvent, int maxConcurrentCalls = 1 )
        {
            var options = new OnMessageOptions { AutoComplete = false, MaxConcurrentCalls = maxConcurrentCalls };

            Client.OnMessage(message =>
            {
                var request = message.GetBody<Request>();

                SafeProcessRequest(requestDelegate, request, message);
            }, options);

            stopEvent.WaitOne();
        }

        public void HandleOne(RequestDelegate requestDelegate)
        {
            BrokeredMessage message;
            var request = GetOneMessage<Request>(out message);
            if (request == null) return;

            SafeProcessRequest(requestDelegate, request, message);
        }

        private static void SafeProcessRequest(RequestDelegate requestDelegate, Request request, BrokeredMessage message)
        {
            try
            {
                var successResponse = requestDelegate(request);
                SendResponse(successResponse);
            }
            catch (Exceptions.MovedException exception)
            {
                var oldId = request.KeyValue;
                var failureResponse = new FailureResponse(request, exception.ErrorType)
                {
                    Value = oldId,
                    Message = exception.Message,
                    Key = {Value = exception.NewKeyValue}
                };

                SendResponse(failureResponse);
            }
            catch (Exceptions.MatchException exception)
            {
                var failureResponse = new FailureResponse(request, exception.ErrorType)
                {
                    Message = exception.Message
                };

                SendResponse(failureResponse);
            }
            catch (Exception)
            {
                message.Abandon();
                throw;
            }
            message.Complete();

        }

        public void Close()
        {
            Client.Close();
        }

        private static void SendResponse<T>(T response) where T : Response
        {
            ResponseTopic.Send(response, new Dictionary<string, object> { { "ResponseType", response.ResponseTypeAsString } });
        }
    }
}
