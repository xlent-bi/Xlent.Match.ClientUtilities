using System;
using Xlent.Match.ClientUtilities.Exceptions;
using Xlent.Match.ClientUtilities.Logging;
using Xlent.Match.ClientUtilities.MatchObjectModel;
using NotImplementedException = Xlent.Match.ClientUtilities.Exceptions.NotImplementedException;

namespace Console.SampleAdapter
{
    public class AdapterBusinessLogic
    {
        public static Data GetObjectData(Key key)
        {
            try
            {
//                var objectFromSystem = GetObjectFromAx(key.EntityName, key.Value);
//
//                var data = new Data();
//                data.Properties["DirPartyId"] = objectFromSystem.Id;
//                data.Properties["ParId"] = objectFromSystem.ParId;
//                data.Properties["FirstName"] = objectFromSystem.FirstName;
//                data.Properties["LastName"] = objectFromSystem.LastName;
//                data.Properties["Epost"] = objectFromSystem.EmailAddress;
//
//                return data;

                System.Console.WriteLine("Get request for key = {0} received.", key);

                throw new NotImplementedException("Get not implemented");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to handle GET on Entity: {0}.", key.EntityName);

                if (e is MatchException) throw;
                throw new InternalServerErrorException(e);
            }
        }

        public static void UpdateObject(Key key, Data data)
        {
            try
            {
                throw new NotImplementedException("Update not implemented");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to handle UPDATE on Entity: {0} of type: {1}.", key.Value, key.EntityName);

                if (e is MatchException) throw;
                throw new InternalServerErrorException(e);
            }
        }

        public static Key CreateObject(Key key, Data data)
        {
            try
            {
                throw new NotImplementedException("Create not implemented");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to handle CREATE on Entity: {0}.", key.EntityName);

                if (e is MatchException) throw;
                throw new InternalServerErrorException(e);
            }
        }
    }
}
