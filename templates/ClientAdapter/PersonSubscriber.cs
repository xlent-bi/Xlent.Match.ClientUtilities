using System;
using System.Threading.Tasks;
using Xlent.Match.ClientUtilities.MatchObjectModel;
using Xlent.Match.ClientUtilities.Messages;

namespace ClientAdapter
{
    public class PersonsSubscriber : BaseSubscriber
    {
        /// <summary>
        /// This is the eternal loop that will receive all requests and call <see cref="HandleRequest"/> for each.
        /// </summary>
        public static async Task HandleRequests()
        {
            await Task.Run(() => BaseSubscriber.HandleRequests("Person", HandleOne));
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
            Business.Person person = null;
            try
            {
                person = Business.Person.Get(int.Parse(request.KeyValue));
            } catch (Exception)
            {
            }
            switch (request.RequestType)
            {
                case "Get":
                    if (person == null) throw new Xlent.Match.ClientUtilities.Exceptions.NotFoundException(request.ClientName, request.EntityName, request.KeyValue);
                    response.Data = new Data();
                    Copy(person.Model, response.Data);
                    break;
                case "Update":
                    if (person == null) throw new Xlent.Match.ClientUtilities.Exceptions.NotFoundException(request.ClientName, request.EntityName, request.KeyValue);
                    Copy(request.Data, person.Model);
                    break;
                case "Create":
                    if (person != null) throw new Xlent.Match.ClientUtilities.Exceptions.MovedException(person.Model.Id.ToString());
                    person = Business.Person.Create();
                    Copy(request.Data, person.Model);
                    response.Key.Value = person.Model.Id.ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("request", string.Format("Unknown request type: \"{0}\"", request.RequestType));
            }

            return response;
        }

        private static void Copy(Model.Person model, Data data)
        {
            data.SetPropertyValue("Id", model.Id.ToString());
            data.SetPropertyValue("FirstName", model.FirstName);
            data.SetPropertyValue("LastName", model.LastName);
        }

        private static void Copy(Data data, Model.Person model)
        {
            model.FirstName = data.GetPropertyValue("FirstName", true);
            model.LastName = data.GetPropertyValue("LastName", true);
        }
    }
}
