using System;
using System.Runtime.Serialization;
using Xlent.Match.ClientUtilities.MatchObjectModel;

namespace Xlent.Match.ClientUtilities.Messages
{
    /// <summary>
    /// Use this message whenever one of your synchronized objects has changed, to make Match initiate its processing.
    /// </summary>
    [DataContract(Name = "Event", Namespace = "http://xlentmatch.com/")]
    public class Event
    {
        public const string Updated = "Updated";
        public const string Moved = "Moved";
        public const string Deleted = "Deleted";

        /// <summary>
        /// The type of event. Mandatory, one of <see cref="Updated"/>, <see cref="Moved"/>, <see cref="Deleted"/>.
        /// </summary>
        [DataMember]
        public string EventType { get; set; }

        /// <summary>
        /// Information about the object that has changed.
        /// The <see cref="MainKey"/> part of the <see cref="MatchObject"/> is mandatory.
        /// The <see cref="ObjectData"/> part is normally left empty, but can be filled in for
        /// events of type <see cref="Updated"/> to avoid being called by Match later.
        /// Mandatory.
        /// </summary>
        /// <remarks>
        /// For an event of type <see cref="Moved"/>, this contains the new identity for the object.
        /// </remarks>
        [DataMember]
        public MatchObject MatchObject { get; set; }

        /// <summary>
        /// Mandatory for events of type <see cref="Moved"/>. It should hold the old object identifier,
        /// before the move.
        /// </summary>
        [DataMember]
        public string OldId { get; set; }

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
        public Event(string eventType)
        {
#if DEBUG
            switch (eventType)
            {
                case Event.Updated:
                case Event.Moved:
                case Event.Deleted:
                    break;
                default:
                    throw new ArgumentException(String.Format("Unknown event type: \"{0}\".", eventType));
            }
#endif
            EventType = eventType;
        }
    }
}
