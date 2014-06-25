using System;
using System.Runtime.Serialization;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities.Messages
{
    /// <summary>
    /// Use this message whenever one of your synchronized objects has changed, to make Match initiate its processing.
    /// </summary>
    [DataContract(Name = "Event", Namespace = "http://xlentmatch.com/")]
    public class Event : IKeyMessage
    {
        public const string Updated = "Updated";
        public const string Moved = "Moved";
        public const string Deleted = "Deleted";

        public enum EventTypeEnum { Updated, Moved, Deleted };

        /// <summary>
        /// The type of event. Mandatory, one of <see cref="Event.Updated"/>,
        /// <see cref="Event.Moved"/>, <see cref="Event.Deleted"/>.
        /// </summary>
        [DataMember]
        public string EventTypeAsString { get; private set; }
        
        public EventTypeEnum EventType { get { return TranslateEventType(EventTypeAsString); } }

        /// <summary>
        /// The identity of the object that has changed.
        /// Mandatory.
        /// </summary>
        /// <remarks>
        /// For an event of type <see cref="Moved"/>, this contains the old identity for the object.
        /// </remarks>
        [DataMember]
        public Key Key { get; set; }

        /// <summary>
        /// Information about the object that has changed.
        /// This is normally left empty, but can be filled in for events of type <see cref="Updated"/> to avoid being called by Match later.
        /// </summary>
        [DataMember]
        public Data Data { get; set; }

        /// <summary>
        /// Mandatory for events of type <see cref="Moved"/>. It should hold the new object identifier,
        /// after the move.
        /// </summary>
        [DataMember]
        public string NewId { get; set; }

        /// <summary>
        /// Best: The time when the event took place in the system.
        /// Good: The time when the event message was created.
        /// Optional.
        /// </summary>
        /// <remarks>
        /// Formatted as http://en.wikipedia.org/wiki/ISO_8601.
        /// Example: 2013-12-20T14:45:15Z
        /// </remarks>
        [DataMember]
        public string TimeStamp { get; set; }

        /// <summary>
        /// The user name of the user that made the change that triggered the event.
        /// Optional.
        /// </summary>
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// A reference if you need to track the event inside match. 
        /// Optional.
        /// </summary>
        [DataMember]
        public string ExternalReference { get; set; }

        /// <summary>
        /// Constructor for a new event.
        /// </summary>
        /// <param name="eventType">The type of event, one of <see cref="Event.Updated"/>, <see cref="Event.Moved"/>
        /// and <see cref="Event.Deleted"/>.</param>
        public Event(EventTypeEnum eventType)
        {
            EventTypeAsString = TranslateEventType(eventType);
        }

        /// <summary>
        /// Translate from <see cref="EventTypeEnum"/> to a string.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <returns>A string representation of the <paramref name="eventType"/>.</returns>
        public static string TranslateEventType(EventTypeEnum eventType)
        {
            switch (eventType)
            {
                case EventTypeEnum.Updated:
                    return Updated;
                case EventTypeEnum.Moved:
                    return Moved;
                case EventTypeEnum.Deleted:
                    return Deleted;
                default:
                    throw new ArgumentException(String.Format("Unknown event type: \"{0}\".", eventType));
            }
        }

        /// <summary>
        /// Translate from a string to <see cref="EventTypeEnum"/>.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <returns>The enumeration value for <paramref name="eventType"/>.</returns>
        public static EventTypeEnum TranslateEventType(string eventType)
        {
            switch (eventType)
            {
                case Updated:
                    return EventTypeEnum.Updated;
                case Moved:
                    return EventTypeEnum.Moved;
                case Deleted:
                    return EventTypeEnum.Deleted;
                default:
                    throw new ArgumentException(String.Format("Unknown event type: \"{0}\".", eventType));
            }
        }

        public override string ToString()
        {
            return NewId != null ? 
                String.Format("[Event {0} {1} (to {2})]", EventTypeAsString, Key, NewId) 
                : String.Format("[Event {0} {1}]", EventTypeAsString, Key);
        }
    }
}
