using Amazon.Runtime.EventStreams;
using static GGCSharp.GreengrassEventStream;

namespace GGCSharp.Events
{
    public class GreengrassUnknownEventStreamEvent : IGreengrassEvent
    {
        public IEventStreamMessage ReceivedMessage { get; set; }

        public string EventType { get; set; }

        public GreengrassUnknownEventStreamEvent()
        {
        }

        public GreengrassUnknownEventStreamEvent(IEventStreamMessage receivedMessage) : this(receivedMessage,
            receivedMessage.Headers[EventTypeHeaderName].AsString())
        {
        }

        public GreengrassUnknownEventStreamEvent(IEventStreamMessage receivedMessage, string eventType)
        {
            this.ReceivedMessage = receivedMessage;
            this.EventType = eventType;
        }
    }
}