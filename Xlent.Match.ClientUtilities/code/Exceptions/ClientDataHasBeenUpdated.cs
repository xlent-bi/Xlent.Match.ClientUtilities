using System;
using Xlent.Match.ClientUtilities.MatchObjectModel;
using Xlent.Match.ClientUtilities.Messages;

namespace Xlent.Match.ClientUtilities.Exceptions
{
    public class ClientDataHasBeenUpdated : MatchException
    {
        public ClientDataHasBeenUpdated(Data newData)
            : base(
                FailureResponse.ErrorTypeEnum.HasBeenUpdated,
                "The client data has been updated, so the update request was denied.")
        {
            if (newData == null)
            {
                throw new ArgumentNullException("newData");
            }

            if (newData.CheckSum == null)
            {
                throw new ArgumentException("Expected to have the Checksum property set. Call newData.CalculateCheckSum() before throwing this exception.", "newData");
            }

            NewCheckSum = newData.CheckSum;
            NewClientData = newData;
        }

        public ClientDataHasBeenUpdated(string newCheckSum)
            : base(
                FailureResponse.ErrorTypeEnum.HasBeenUpdated,
                "The client data has been updated, so the update request was denied.")
        {
            NewCheckSum = newCheckSum;
            NewClientData = null;
        }

        public string NewCheckSum { get; private set; }
        public Data NewClientData { get; private set; }
    }
}