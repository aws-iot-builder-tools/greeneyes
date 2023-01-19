using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Runtime.EventStreams;
using static GGCSharp.GreengrassEventStream;

namespace GGCSharp
{
    public static class RpcMessages
    {
        public static EventStreamMessage ToEventStreamMessage<T>(T RpcMessage, MessageType messageType,
            int messageFlags, int streamId) where T : RpcMessage
        {
            var headers = new List<IEventStreamHeader>();

            if (RpcMessage is Connect)
            {
                var versionHeader = new EventStreamHeader(VersionHeaderName);
                versionHeader.SetString(VersionHeaderValue);
                headers.Add(versionHeader);
            }

            if (RpcMessage is RpcApplicationMessage message1)
            {
                var contentTypeHeader = new EventStreamHeader(ContentTypeHeaderName);
                contentTypeHeader.SetString(JsonContentTypeHeaderValue);
                headers.Add(contentTypeHeader);

                var serviceModelTypeHeader = new EventStreamHeader(ServiceModelTypeHeaderName);
                serviceModelTypeHeader.SetString(message1.applicationModelType);
                headers.Add(serviceModelTypeHeader);
            }

            var messageTypeHeader = new EventStreamHeader(MessageTypeHeaderName);
            messageTypeHeader.SetInt32((int) messageType);
            headers.Add(messageTypeHeader);

            var messageFlagsHeader = new EventStreamHeader(MessageFlagsHeaderName);
            messageFlagsHeader.SetInt32(messageFlags);
            headers.Add(messageFlagsHeader);

            var streamIdHeader = new EventStreamHeader(StreamIdHeaderName);
            streamIdHeader.SetInt32(streamId);
            headers.Add(streamIdHeader);

            if (RpcMessage is RpcApplicationMessage message2)
            {
                var operationHeader = new EventStreamHeader(OperationHeaderName);
                operationHeader.SetString(message2.operationType);
                headers.Add(operationHeader);
            }

            var json = JsonSerializer.Serialize(RpcMessage);
            json = json.Replace("=", "\\u003d");
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            return new EventStreamMessage(headers, jsonBytes);
        }
    }

    public enum MessageType : byte
    {
        ApplicationMessage = 0,
        ApplicationError = 1,
        Ping = 2,
        PingResponse = 3,
        Connect = 4,
        ConnectAck = 5,
        ProtocolError = 6,
        ServerError = 7
    }

    public class RpcMessage
    {
    }

    public abstract class RpcApplicationMessage : RpcMessage
    {
        // NOTE: This must start with a lowercase character to make sure it gets serialized that way
        public abstract string applicationModelType { get; }
        // NOTE: This must start with a lowercase character to make sure it gets serialized that way
        public abstract string operationType { get; }
    }

    public class Connect : RpcMessage
    {
        // NOTE: This must start with a lowercase character to make sure it gets serialized that way
        public string authToken { get; }

        public Connect(string authToken)
        {
            this.authToken = authToken;
        }
    }

    public class PublishToIoTCore : RpcApplicationMessage
    {
        // NOTE: This must start with a lowercase character to make sure it gets serialized that way
        public string topicName { get; }
        // NOTE: This must start with a lowercase character to make sure it gets serialized that way
        public string qos { get; }
        // NOTE: This must start with a lowercase character to make sure it gets serialized that way
        public byte[] payload { get; }

        public PublishToIoTCore(string topicName, string qos, byte[] payload)
        {
            this.topicName = topicName;
            this.qos = qos;
            this.payload = payload;
        }

        [JsonIgnore] public override string operationType => "aws.greengrass#PublishToIoTCore";
        [JsonIgnore] public override string applicationModelType => "aws.greengrass#PublishToIoTCoreRequest";
    }
}