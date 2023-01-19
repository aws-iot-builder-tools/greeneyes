using System;
using System.Collections.Generic;
using System.Text.Json;
using Amazon.Runtime.EventStreams;
using static GGCSharp.GreengrassEventStream;

namespace GGCSharp.Events
{
    public class GreengrassApplicationErrorEvent : IGreengrassEvent
    {
        private readonly Dictionary<string, object> _errorInformation;

        public Dictionary<string, object> ErrorInformation => _errorInformation;

        public GreengrassApplicationErrorEvent()
        {
        }

        public GreengrassApplicationErrorEvent(IEventStreamMessage message)
        {
            if (!message.Headers.ContainsKey(ContentTypeHeaderName))
            {
                return;
            }

            var contentType = message.Headers[ContentTypeHeaderName].AsString();

            if (contentType != JsonContentTypeHeaderValue)
            {
                return;
            }

            var payloadString = System.Text.Encoding.UTF8.GetString(message.Payload);
            _errorInformation = JsonSerializer.Deserialize<Dictionary<string, object>>(payloadString);

            Console.WriteLine($"Application error: {payloadString}");
        }
    }
}