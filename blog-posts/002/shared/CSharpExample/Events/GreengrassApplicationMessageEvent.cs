using System;
using Amazon.Runtime.EventStreams;

namespace GGCSharp.Events
{
    public class GreengrassApplicationMessageEvent : IGreengrassEvent
    {
        public GreengrassApplicationMessageEvent()
        {
        }

        public GreengrassApplicationMessageEvent(IEventStreamMessage message)
        {
            Console.WriteLine("About to print payload");
            Console.WriteLine($"Application message: {message.Payload}");
        }
    }
}