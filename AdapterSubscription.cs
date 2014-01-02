using System;
using System.Collections.Generic;
using Microsoft.ServiceBus.Messaging;
using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities
{
    public class AdapterSubscription : ServiceBus.Subscription
    {
        private static ServiceBus.Topic _requestTopic;
        private static ServiceBus.Topic _responseTopic;

        static AdapterSubscription()
        {
            _requestTopic = new ClientUtilities.ServiceBus.Topic("Xlent.Match.ClientUtilities.ConnectionString", "Request");
            _responseTopic = new ServiceBus.Topic("Xlent.Match.ClientUtilities.ConnectionString", "Response");
        }

        private AdapterSubscription(string name, Filter filter)
            : base(_requestTopic, name, filter)
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

        public delegate Messages.SuccessResponse RequestDelegate(Request request);

        public void HandleOne(RequestDelegate requestDelegate)
        {
            BrokeredMessage message;
            var request = GetOneMessage<Request>(out message);
            if (request == null) return;

            try
            {
                var successResponse = requestDelegate(request);
                SendResponse(successResponse);
            }
            catch (Exceptions.MovedException exception)
            {
                var failureResponse = new Messages.FailureResponse(request, exception.ErrorLevel, exception.ErrorType)
                {
                    Value = exception.NewKeyValue,
                    Message = exception.Message
                };

                SendResponse(failureResponse);
            }
            catch (Exceptions.BaseClass exception)
            {
                var failureResponse = new Messages.FailureResponse(request, exception.ErrorLevel, exception.ErrorType)
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

        private static void SendResponse<T>(T response) where T : Response
        {
            _responseTopic.Send<T>(response, new Dictionary<string, object>() { { "ResponseType", response.ResponseType } });
        }
    }
}
