using System;
using Amazon.Runtime.EventStreams;

namespace GGCSharp.Events
{
    public class GreengrassConnectAckEvent : IGreengrassEvent
    {
        public bool Success { get; }

        public GreengrassConnectAckEvent()
        {
        }

        public GreengrassConnectAckEvent(IEventStreamMessage message)
        {
            Success = message.Headers[GreengrassEventStream.MessageFlagsHeaderName].AsInt32() == 1;
        }
    }
}