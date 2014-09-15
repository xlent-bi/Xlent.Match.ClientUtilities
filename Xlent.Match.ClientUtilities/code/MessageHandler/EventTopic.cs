using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using Xlent.Match.ClientUtilities.Logging;
using Xlent.Match.ClientUtilities.MatchObjectModel;
using Xlent.Match.ClientUtilities.Messages;
using Xlent.Match.ClientUtilities.ServiceBus;

namespace Xlent.Match.ClientUtilities.MessageHandler
{
    public class EventTopic
    {
        private static Topic _topic;

        public EventTopic(string clientName)
        {
            ClientName = clientName;
        }

        public static Topic Topic
        {
            get
            {
                if (_topic != null)
                    return _topic;

                var pairedConnectionString =
                    ConfigurationManager.AppSettings["Xlent.Match.ClientUtilities.PairedConnectionString"];
                _topic = ! String.IsNullOrEmpty(pairedConnectionString)
                    ? new Topic("Xlent.Match.ClientUtilities.ConnectionString",
                        "Xlent.Match.ClientUtilities.PairedConnectionString", "Event")
                    : new Topic("Xlent.Match.ClientUtilities.ConnectionString", "Event");

                return _topic;
            }

            set { _topic = value; }
        }

        public string ClientName { get; private set; }

        public static async Task<long> TopicLengthAsync()
        {
            return await Topic.GetLengthAsync();
        }

        public static async Task SendAsync(Event theEvent)
        {
            Log.Verbose("==> Sending {0}", theEvent);
            await Topic.SendAsync(theEvent, new Dictionary<string, object> {{"Type", theEvent.EventTypeAsString}});
        }

        public async Task SendUpdatedAsync(string entityName, string keyValue, string userName = null, DateTime? timeStamp = null,
            string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Updated)
            {
                Key = new Key(ClientName, entityName, keyValue)
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            await SendAsync(theEvent);
        }

        public async Task SendDeletedAsync(string entityName, string keyValue, string userName = null, DateTime? timeStamp = null,
            string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Deleted)
            {
                Key = new Key(ClientName, entityName, keyValue)
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            await SendAsync(theEvent);
        }

        public async Task SendMoved(string entityName, string oldKeyValue, string newKeyValue, string userName = null,
            DateTime? timeStamp = null, string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Moved)
            {
                Key = new Key(ClientName, entityName, oldKeyValue),
                NewId = newKeyValue
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            await SendAsync(theEvent);
        }

        private static void AddOptionalFields(Event theEvent, string userName = null, DateTime? timeStamp = null,
            string externalReference = null)
        {
            theEvent.UserName = userName;
            theEvent.ExternalReference = externalReference;
            if (null != timeStamp)
            {
                theEvent.TimeStamp = ((DateTime) timeStamp).ToUniversalTime().ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}