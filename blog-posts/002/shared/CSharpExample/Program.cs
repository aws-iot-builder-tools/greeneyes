using System;
using System.Diagnostics;
using System.Threading;

namespace GGCSharp
{
    public static class Program
    {
        public static bool Running { get; set; } = true;
        private static string AwsIotThingName => Environment.GetEnvironmentVariable("AWS_IOT_THING_NAME");
        public static string ThingTopic => string.Join("/", "greengrass", AwsIotThingName);

        private static void Main(string[] args)
        {
            InstallShutdownHandler();

            while (Running)
            {
                var currentProcess = Process.GetCurrentProcess();
                AwsIotGreengrassIpc.Publish(ThingTopic, 0, $"Hello from C# process: {currentProcess.WorkingSet64}");
                Thread.Sleep(10000);
            }

            AwsIotGreengrassIpc.Publish(ThingTopic, 1, "Shutting down");
        }

        private static void InstallShutdownHandler()
        {
            // If the process is exiting set the global running flag to false
            AppDomain.CurrentDomain.ProcessExit += (_, _) => Running = false;
        }
    }
}