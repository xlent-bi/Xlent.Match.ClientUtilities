using System;
using System.Configuration;
using Xlent.Match.ClientUtilities;
using Xlent.Match.Test.ClientAdapter;

namespace Xlent.Match.Test.ClientAdapter
{
    public class BusinessLogic
    {

        public BusinessLogic()
        {
            _fileStorage = new XmlMatchObjectFileStorage(ConfigurationManager.AppSettings["basePath"]);
        }

        public ClientUtilities.MatchObjectModel.MatchObject GetObject(ClientUtilities.MatchObjectModel.Key mainKey)
        {
            var matchObject = GetMatchObject(mainKey);

            MaybeForceError(mainKey, new MatchObjectControl(matchObject), ClientAdapter.Utils.MatchObjectHelper.ForceGetErrorCode);

            return matchObject;
        }

        public void UpdateObject(ClientUtilities.Messages.Request request)
        {
            var mainKey = request.MatchObject.Key;
            var oldMatchObject = GetMatchObject(mainKey);

            MaybeForceError(mainKey, new MatchObjectControl(oldMatchObject), ClientAdapter.Utils.MatchObjectHelper.ForceUpdateErrorCode);

            _fileStorage.Update(mainKey.ClientName, mainKey.EntityName, mainKey.Value, request.MatchObject);
        }

        public string CreateObject(ClientUtilities.Messages.Request request)
        {
            MaybeForceError(request.MatchObject.Key, new MatchObjectControl(request.MatchObject), ClientAdapter.Utils.MatchObjectHelper.ForceCreateErrorCode);

            var mainKey = request.MatchObject.Key;
            return _fileStorage.Create(mainKey.ClientName, mainKey.EntityName, request.MatchObject);
        }

        private ClientUtilities.MatchObjectModel.MatchObject GetMatchObject(ClientUtilities.MatchObjectModel.Key mainKey)
        {
            var matchObject = _fileStorage.XmlDocumentToMatchObject(mainKey.ClientName, mainKey.EntityName, mainKey.Value);

            if (matchObject == null)
            {
                throw new ClientUtilities.Exceptions.NotFoundException(mainKey.ClientName, mainKey.EntityName, mainKey.Value);
            }

            return matchObject;
        }

        private static void MaybeForceError(
            ClientUtilities.MatchObjectModel.Key mainKey,
            ClientUtilities.MatchObjectControl matchObjectControl,
            string errorProperty)
        {
            string fakeErrorCode = matchObjectControl.GetPropertyValue(errorProperty, true);

            if ((fakeErrorCode == null) || (fakeErrorCode.Length != 3))
            {
                return;
            }

            var value = matchObjectControl.GetPropertyValue(ClientAdapter.Utils.MatchObjectHelper.ForceErrorValue, true);

            // fake for testing purposes. looks like MS dowsn't support 422, 429. at least not in the System.Net.HttpStatusCode enum
            switch (fakeErrorCode)
            {
                case "301":
                    throw new ClientUtilities.Exceptions.MovedException(value);
                case "400":
                    throw new ClientUtilities.Exceptions.BadRequestException("Bad request reason.");
                case "403":
                    throw new ClientUtilities.Exceptions.ForbiddenException("Forbidden reason.");
                case "404":
                    throw new ClientUtilities.Exceptions.NotFoundException(mainKey.ClientName, mainKey.EntityName, mainKey.Value);
                case "406":
                    throw new ClientUtilities.Exceptions.NotAcceptableException("Not acceptable reason.");
                case "410":
                    throw new ClientUtilities.Exceptions.GoneException(mainKey.ClientName, mainKey.EntityName, mainKey.Value);
                case "422":
                    throw new ClientUtilities.Exceptions.GoneException(mainKey.ClientName, mainKey.EntityName, mainKey.Value);
                case "500":
                    throw new ClientUtilities.Exceptions.InternalServerErrorException("Internal server error reason.");
                case "501":
                    throw new ClientUtilities.Exceptions.NotImplementedException("Not implemented description.");
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Fake error code {0} unknown.", fakeErrorCode));
            }
        }
    }
}
