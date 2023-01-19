using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.Common;

namespace GGCSharp
{
    public static class AwsIotGreengrassIpc
    {
        public static Task<Result<Unit>> Publish(string topicName, int qos, string payload)
        {
            return Task.Run(() => InnerPublish(topicName, qos, payload));
        }

        private static Result<Unit> InnerPublish(string topicName, int qos, string payload)
        {
            var awsIotGreengrassIpcPipe = new AwsIotGreengrassIpcPipe();

            // Publish a message to IoT Core (will connect automatically)
            awsIotGreengrassIpcPipe.PublishToIotCore(topicName, qos, payload);
            Thread.Sleep(1000);
            awsIotGreengrassIpcPipe.Close();

            return Unit.Default;
        }
    }
}