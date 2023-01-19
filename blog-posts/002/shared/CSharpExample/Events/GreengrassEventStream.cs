using System;
using System.Collections.Generic;
using System.IO;
using Amazon.Runtime.EventStreams;
using Amazon.Runtime.EventStreams.Internal;
using GGCSharp.Events;

namespace GGCSharp
{
    public class GreengrassEventStream : EnumerableEventStream<IGreengrassEvent, GreengrassEventStreamException>
    {
        public const string MessageTypeHeaderName = ":message-type";
        public const string MessageFlagsHeaderName = ":message-flags";
        public const string EventTypeHeaderName = ":event-type";
        public const string VersionHeaderName = ":version";
        public const string VersionHeaderValue = "0.1.0";
        public const string ContentTypeHeaderName = ":content-type";
        public const string JsonContentTypeHeaderValue = "application/json";
        public const string ServiceModelTypeHeaderName = "service-model-type";
        public const string StreamIdHeaderName = ":stream-id";
        public const string OperationHeaderName = "operation";

        public event EventHandler<EventStreamEventReceivedArgs<GreengrassConnectAckEvent>> GreengrassConnectAckReceived;
        public event EventHandler<EventStreamEventReceivedArgs<UnauthorizedErrorEvent>> UnauthorizedErrorReceived;

        public GreengrassEventStream(Stream stream) : base(stream)
        {
        }

        public override event EventHandler<EventStreamEventReceivedArgs<IGreengrassEvent>> EventReceived;

        public override event EventHandler<EventStreamExceptionReceivedArgs<GreengrassEventStreamException>>
            ExceptionReceived;


        public GreengrassEventStream(Stream? stream, IEventStreamDecoder eventStreamDecoder) : base(stream,
            eventStreamDecoder)
        {
            // See https://github.com/aws/aws-sdk-net/blob/475822dec5e87954b7a47ac65995714ae1f1b115/sdk/src/Services/S3/Custom/Model/Events/SelectObjectContentEventStream.cs#L190
            //   for an explanation of why this is necessary
            base.EventReceived += (sender, args) => EventReceived?.Invoke(this, args);
            base.ExceptionReceived += (sender, args) => ExceptionReceived?.Invoke(this, args);

            // Mapping the generic event to more specific events
            Decoder.MessageReceived += (sender, args) =>
            {
                var ev = ConvertMessageToEvent(args.Message);

                EventReceived?.Invoke(this, new EventStreamEventReceivedArgs<IGreengrassEvent>(ev));

                // Call RaiseEvent until it returns true or all calls complete. This way, only a subset of casts is preformed,
                // and we can avoid a cascade of nested if/else statements. The result is thrown away.
                var _ = RaiseEvent(GreengrassConnectAckReceived, ev);
                
                // TODO: Other events have not been implemented yet. Example events from S3 below.
                //   RaiseEvent(RecordsEventReceived, ev) ||
                //   RaiseEvent(StatsEventReceived, ev) ||
                //   RaiseEvent(ProgressEventReceived, ev) ||
                //   RaiseEvent(ContinuationEventReceived, ev) ||
                //   RaiseEvent(EndEventReceived, ev);
            };
        }

        /// <summary>
        /// Converts an event stream message to a Greengrass event. This hides the original implementation in the SDK
        /// since that implementation makes certain assumptions about the message headers that aren't valid in the
        /// Greengrass IPC protocol.
        /// </summary>
        /// <param name="eventStreamMessage"></param>
        /// <returns></returns>
        private IGreengrassEvent ConvertMessageToEvent(EventStreamMessage eventStreamMessage)
        {
            var headers = eventStreamMessage.Headers;

            // The value is sent as an int32, but it is only a byte
            var messageTypeByte = (byte) headers[MessageTypeHeaderName].AsInt32();

            if (!Enum.IsDefined(typeof(MessageType), messageTypeByte))
            {
                throw new UnknownEventStreamMessageTypeException();
            }

            var messageType = (MessageType) messageTypeByte;

            try
            {
                return EventMapping[messageType.ToString()](eventStreamMessage);
            }
            catch (KeyNotFoundException)
            {
                return EventMapping[UnknownEventKey](eventStreamMessage);
            }
        }

        private bool RaiseEvent<T>(EventHandler<EventStreamEventReceivedArgs<T>> eventHandler, IGreengrassEvent ev)
            where T : class, IGreengrassEvent
        {
            var convertedEvent = ev as T;
            if (convertedEvent != null)
            {
                eventHandler?.Invoke(this, new EventStreamEventReceivedArgs<T>(convertedEvent));
                return true;
            }

            return false;
        }

        /// <summary>
        /// The mapping of event message to a generator function to construct the matching Event Stream event.
        /// </summary>
        protected override IDictionary<string, Func<IEventStreamMessage, IGreengrassEvent>> EventMapping { get; } =
            new Dictionary<string, Func<IEventStreamMessage, IGreengrassEvent>>
            {
                {UnknownEventKey, payload => new GreengrassUnknownEventStreamEvent(payload)},
                {MessageType.ConnectAck.ToString(), payload => new GreengrassConnectAckEvent(payload)},
                {MessageType.ApplicationMessage.ToString(), payload => new GreengrassApplicationMessageEvent(payload)},
                {MessageType.ApplicationError.ToString(), payload => new GreengrassApplicationErrorEvent(payload)}
                // {"Records", payload => new RecordsEvent(payload)},
                // {"Stats", payload => new StatsEvent(payload)},
                // {"Progress", payload => new ProgressEvent(payload)},
                // {"Cont", payload => new ContinuationEvent(payload)},
                // {"End", payload => new EndEvent(payload)}
            };

        protected override IDictionary<string, Func<IEventStreamMessage, GreengrassEventStreamException>>
            ExceptionMapping { get; }

        protected override bool IsProcessing { get; set; }
    }
}