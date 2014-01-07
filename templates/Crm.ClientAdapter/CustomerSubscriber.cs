using System;
using System.Threading.Tasks;
using Xlent.Match.ClientUtilities.MatchObjectModel;
using Xlent.Match.ClientUtilities.Messages;

namespace Crm.ClientAdapter
{
    public class CustomerSubscriber : BaseSubscriber
    {
        /// <summary>
        /// This is the eternal loop that will receive all requests and call <see cref="HandleRequest"/> for each.
        /// </summary>
        public static async Task HandleRequests()
        {
            await Task.Run(() => BaseSubscriber.HandleRequests("Customer", HandleOne));
        }

        /// <summary>
        /// Handle one request.
        /// </summary>
        /// <param name="request">The request to handle.</param>
        /// <returns>A successful response.</returns>
        /// <remarks>If the method fails, it will throw an exception, corresponding to the failure types
        /// in http://xlentmatch.com/wiki/FailureResponse_Message#Error_Types</remarks>
        private static SuccessResponse HandleOne(Request request)
        {
            var response = new SuccessResponse(request);
            Business.Customer customer = null;
            try
            {
                customer = Business.Customer.Get(int.Parse(request.KeyValue));
            } catch (Exception)
            {
            }
            switch (request.RequestType)
            {
                case "Get":
                    if (customer == null) throw new Xlent.Match.ClientUtilities.Exceptions.NotFoundException(request.ClientName, request.EntityName, request.KeyValue);
                    response.Data = new Data();
                    Copy(customer.Model, response.Data);
                    break;
                case "Update":
                    if (customer == null) throw new Xlent.Match.ClientUtilities.Exceptions.NotFoundException(request.ClientName, request.EntityName, request.KeyValue);
                    Copy(request.Data, customer.Model);
                    break;
                case "Create":
                    if (customer != null) throw new Xlent.Match.ClientUtilities.Exceptions.MovedException(customer.Model.Id.ToString());
                    customer = Business.Customer.Create();
                    Copy(request.Data, customer.Model);
                    response.Key.Value = customer.Model.Id.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("request", string.Format("Unknown request type: \"{0}\"", request.RequestType));
            }

            return response;
        }

        private static void Copy(Model.Customer model, Data data)
        {
            data.SetPropertyValue("Id", model.Id.ToString());
            data.SetPropertyValue("CustomerNumber", model.CustomerNumber);
            data.SetPropertyValue("PersonId", model.PersonId.ToString());
        }

        private static void Copy(Data data, Model.Customer model)
        {
            model.CustomerNumber = data.GetPropertyValue("CustomerNumber", true);
            model.PersonId = int.Parse(data.GetPropertyValue("PersonId", true));
        }
    }
}
