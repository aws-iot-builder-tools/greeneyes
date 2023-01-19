using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using Amazon.Runtime.EventStreams.Internal;
using LanguageExt;
using LanguageExt.Common;

namespace GGCSharp
{
    public class AwsIotGreengrassIpcPipe
    {
        public static string UNIX_SOCKET_ENVIRONMENT_VARIABLE_NAME =
            "AWS_GG_NUCLEUS_DOMAIN_SOCKET_FILEPATH_FOR_COMPONENT";

        public static string AUTH_TOKEN_ENVIRONMENT_VARIABLE_NAME =
            "SVCUID";

        private static readonly string IpcPipeName =
            Environment.GetEnvironmentVariable(UNIX_SOCKET_ENVIRONMENT_VARIABLE_NAME) ??
            throw new Exception(
                $"Could not determine the location of the Unix domain socket for Greengrass IPC because the {UNIX_SOCKET_ENVIRONMENT_VARIABLE_NAME} environment variable is missing");

        private static readonly string IpcAuthToken = Environment.GetEnvironmentVariable("SVCUID") ??
                                                      throw new Exception(
                                                          $"No IPC authentication token was found because {AUTH_TOKEN_ENVIRONMENT_VARIABLE_NAME} environment variable is missing");

        private int _streamId = 0;

        private GreengrassEventStream GreengrassEventStream { get; set; }

        private bool Connected { get; set; }

        private NamedPipeClientStream? _pipeClient = null;

        public Result<Unit> Connect()
        {
            if (Connected)
            {
                Console.WriteLine("Already connected.");
                return Unit.Default;
            }

            if (!File.Exists(IpcPipeName))
            {
                var message = $"{IpcPipeName} does not exist, is Greengrass running?";
                throw new Exception(message);
            }

            _pipeClient = new NamedPipeClientStream(".", IpcPipeName);
            _pipeClient.Connect();

            GreengrassEventStream = new GreengrassEventStream(_pipeClient, new EventStreamDecoder());

            IEnumerator<IGreengrassEvent> greengrassEventEnumerator = null;

            // TODO: Do something with the received events
            GreengrassEventStream.EventReceived += (sender, receivedArgs) =>
                Console.WriteLine($"Event: {receivedArgs} {sender}");
            // TODO: Do something with the received exceptions
            GreengrassEventStream.ExceptionReceived +=
                (sender, receivedArgs) => Console.WriteLine($"Exception: {receivedArgs} {sender}");

            GreengrassEventStream.GreengrassConnectAckReceived += (sender, receivedArgs) =>
                Connected = receivedArgs.EventStreamEvent.Success;

            GreengrassEventStream.StartProcessing();

            var connect = new Connect(IpcAuthToken);
            var connectEventStreamMessage =
                RpcMessages.ToEventStreamMessage(connect, MessageType.Connect, 0, _streamId++);
            var connectBytes = connectEventStreamMessage.ToByteArray();
            _pipeClient.Write(connectBytes);

            // NOTE: When using socat to proxy UNIX domain sockets to MacOS hosts the return path does not work so we
            //         currently don't try to receive the connection ACK here.
            // TODO: Implement receive for connection ACK
            // TODO: Fix bidirectional UNIX domain socket issues when proxying to MacOS with socat

            // Wait for a second here to let Greengrass decide if the client is valid. If we don't sleep here we may keep
            //   sending data down the pipe before Greengrass closes it if the auth is invalid.
            Thread.Sleep(1000);

            return Unit.Default;
        }

        public Result<Unit> Close()
        {
            if (!Connected) return Unit.Default;
            if (_pipeClient == null)
                throw new NullReferenceException("_pipeClient is NULL. This should never happen. This is a bug.");

            Connected = false;
            _pipeClient.Close();

            return Unit.Default;
        }

        public Result<Unit> PublishToIotCore(string topicName, int qos, string payload)
        {
            return PublishToIotCore(topicName, qos, Encoding.UTF8.GetBytes(payload));
        }

        public Result<Unit> PublishToIotCore(string topicName, int qos, byte[] payload)
        {
            if (qos != 0 && qos != 1)
                throw new Exception($"QoS must be either 0 or 1, QoS requested was {qos}");

            // Connect, if necessary
            Connect();

            if (_pipeClient == null)
                throw new NullReferenceException("_pipeClient is NULL. This should never happen. This is a bug.");
            
            // Build a PublishToIotCore message
            var publishToIotCore = new PublishToIoTCore(topicName, qos.ToString(), payload);

            // Format the message as an event stream message
            var publishToIotCoreEventStreamMessage =
                RpcMessages.ToEventStreamMessage(publishToIotCore, MessageType.ApplicationMessage, 0, _streamId++);

            // Convert the message to bytes
            var messageBytes = publishToIotCoreEventStreamMessage.ToByteArray();

            // Write the message to the pipe
            _pipeClient.Write(messageBytes);

            return Unit.Default;
        }
    }
}