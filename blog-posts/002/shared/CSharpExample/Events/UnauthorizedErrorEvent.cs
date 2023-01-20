using System;
using Amazon.Runtime.EventStreams;

namespace GGCSharp.Events
{
    public class UnauthorizedErrorEvent : IGreengrassEvent
    {
        public bool Success { get; set; }

        public UnauthorizedErrorEvent()
        {
        }

        public UnauthorizedErrorEvent(IEventStreamMessage message)
        {
            Success = message.Headers[GreengrassEventStream.MessageFlagsHeaderName].AsInt32() == 1;
        }
    }
}