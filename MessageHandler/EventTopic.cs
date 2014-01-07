using System.Collections.Generic;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using Xlent.Match.ClientUtilities;
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
                if (_topic == null)
                {
                    _topic = new Topic("Xlent.Match.ClientUtilities.ConnectionString", "Event");
                }
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
            Topic.Send(theEvent, new Dictionary<string, object> { { "Type", theEvent.EventType } });
        }

        public void SendUpdated(string entityName, string keyValue, string userName = null, DateTime? timeStamp = null, string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Updated)
            {
                MatchObject = GetMatchObject(ClientName, entityName, keyValue)
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            Send(theEvent);
        }

        public void SendDeleted(string entityName, string keyValue, string userName = null, DateTime? timeStamp = null, string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Deleted)
            {
                MatchObject = GetMatchObject(ClientName, entityName, keyValue)
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            Send(theEvent);
        }

        public void SendMoved(string entityName, string oldKeyValue, string newKeyValue, string userName = null, DateTime? timeStamp = null, string externalReference = null)
        {
            var theEvent = new Event(Event.EventTypeEnum.Moved)
            {
                MatchObject = GetMatchObject(ClientName, entityName, newKeyValue),
                OldId = oldKeyValue
            };

            AddOptionalFields(theEvent, userName, timeStamp, externalReference);

            Send(theEvent);
        }

        private static MatchObjectModel.MatchObject GetMatchObject(string clientName, string entityName, string keyValue)
        {
            var mainKey = new MatchObjectModel.Key()
               {
                   ClientName = clientName,
                   EntityName = entityName,
                   Value = keyValue
               };

            return new MatchObjectModel.MatchObject()
            {
                Key = mainKey
            };
        }

        private static void AddOptionalFields(Event theEvent, string userName = null, DateTime? timeStamp = null, string externalReference = null)
        {
            DateTime time = timeStamp??DateTime.UtcNow;
            theEvent.UserName = userName;
            theEvent.ExternalReference = externalReference;
        }
    }
}

