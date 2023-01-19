using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Runtime.EventStreams;

namespace GGCSharp
{
    public class EventStreamMessageJsonConverter : JsonConverter<IEventStreamMessage>
    {
        public override IEventStreamMessage Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IEventStreamMessage value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            IDictionary<string, string> headerDictionary = new Dictionary<string, string>();

            value.Headers
                .Map(ConvertHeaderToString)
                .Map(ConvertMessageTypeToString)
                .ToList()
                .ForEach(keyValuePair => headerDictionary.Add(keyValuePair));

            writer.WritePropertyName("headers");
            JsonSerializer.Serialize(writer, headerDictionary, options);

            writer.WriteString("payload", System.Text.Encoding.UTF8.GetString(value.Payload));

            writer.WriteEndObject();
        }

        private static KeyValuePair<string, string> ConvertHeaderToString(
            KeyValuePair<string, IEventStreamHeader> header)
        {
            switch (header.Value.HeaderType)
            {
                case EventStreamHeaderType.BoolTrue:
                    return new KeyValuePair<string, string>(header.Key, "true");
                case EventStreamHeaderType.BoolFalse:
                    return new KeyValuePair<string, string>(header.Key, "false");
                case EventStreamHeaderType.Byte:
                    return new KeyValuePair<string, string>(header.Key, header.Value.AsByte().ToString());
                case EventStreamHeaderType.Int16:
                    return new KeyValuePair<string, string>(header.Key, header.Value.AsInt16().ToString());
                case EventStreamHeaderType.Int32:
                    return new KeyValuePair<string, string>(header.Key, header.Value.AsInt32().ToString());
                case EventStreamHeaderType.Int64:
                    return new KeyValuePair<string, string>(header.Key, header.Value.AsInt64().ToString());
                case EventStreamHeaderType.ByteBuf:
                    return new KeyValuePair<string, string>(header.Key,
                        System.Text.Encoding.UTF8.GetString(header.Value.AsByteBuf()));
                case EventStreamHeaderType.String:
                    return new KeyValuePair<string, string>(header.Key, header.Value.AsString());
                case EventStreamHeaderType.Timestamp:
                    return new KeyValuePair<string, string>(header.Key, header.Value.AsTimestamp().ToString());
                case EventStreamHeaderType.UUID:
                    return new KeyValuePair<string, string>(header.Key, header.Value.AsUUID().ToString());
            }

            throw new Exception($"Unexpected type [{header.Value.HeaderType}");
        }

        private static KeyValuePair<string, string> ConvertMessageTypeToString(
            KeyValuePair<string, string> header)
        {
            // No conversion if this isn't a message header
            if (header.Key != GreengrassEventStream.MessageTypeHeaderName) return header;

            return new KeyValuePair<string, string>(header.Key, Enum.Parse(typeof(MessageType), header.Value).ToString());
        }
    }
}