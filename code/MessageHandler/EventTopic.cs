using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using Xlent.Match.ClientUtilities.Messages;
using Xlent.Match.ClientUtilities.ServiceBus;

namespace Xlent.Match.ClientUtilities.MessageHandler
{
    public class EventTopic
    {
        private static Topic _topic;

        public static Topic Topic
        {
            get
            {
                if (_topic != null)
                    return _topic;

                var pairedConnectionString =
                    ConfigurationManager.AppSettings["Xlent.Match.ClientUtilities.PairedConnectionString"];
                _topic = ! String.IsNullOrEmpty(pairedConnectionString) ? new Topic("Xlent.Match.ClientUtilities.ConnectionString", "Xlent.Match.ClientUtilities.PairedConnectionString", "Event") : new Topic("Xlent.Match.ClientUtilities.ConnectionString", "Event");

                return _topic;
            }

            set { _topic = value; }
        }

        public string ClientName { get; private set; }

        public EventTopic(string clientName)
        {
            ClientName = clientName;
        }

        public static long TopicLength()
        {
            return Topic.GetLength();
        }

        public static void Send(Event theEvent)
        {
            Topic.Send(theEvent, new Dictionary<string, object> { { "Type", theEvent.EventTypeAsString } });
        }

        public void SendUpdated(string entityName, string keyValue, string userName = null, DateTime? timeStamp = null, string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Updated)
            {
                Key = new MatchObjectModel.Key(ClientName, entityName, keyValue)
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            Send(theEvent);
        }

        public void SendDeleted(string entityName, string keyValue, string userName = null, DateTime? timeStamp = null, string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Deleted)
            {
                Key = new MatchObjectModel.Key(ClientName, entityName, keyValue)
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            Send(theEvent);
        }

        public void SendMoved(string entityName, string oldKeyValue, string newKeyValue, string userName = null, DateTime? timeStamp = null, string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Moved)
            {
                Key = new MatchObjectModel.Key(ClientName, entityName, newKeyValue),
                OldId = oldKeyValue
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            Send(theEvent);
        }

        private static void AddOptionalFields(Event theEvent, string userName = null, DateTime? timeStamp = null, string externalReference = null)
        {
            theEvent.UserName = userName;
            theEvent.ExternalReference = externalReference;
            if (null != timeStamp)
            {
                theEvent.TimeStamp = ((DateTime)timeStamp).ToUniversalTime().ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}

