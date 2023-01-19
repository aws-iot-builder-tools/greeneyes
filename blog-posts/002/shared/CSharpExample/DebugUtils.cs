using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GGCSharp
{
    public static class DebugUtils
    {
        public static void ShowNewThreadIdsAndProcessorTime(List<ProcessThread> threadsBefore)
        {
            var newThreads = GetNewThreads(threadsBefore);
            ShowThreadIdAndProcessorTime(newThreads);
        }

        public static void ShowNewWaitingThreads(List<ProcessThread> threadsBefore)
        {
            var newThreads = GetNewThreads(threadsBefore);

            foreach (var processThread in newThreads)
            {
                try
                {
                    Console.WriteLine($"{processThread.Id} is waiting because {processThread.WaitReason}");
                }
                catch (Exception e)
                {
                    // Do nothing
                }
            }

            ShowThreadIdAndProcessorTime(newThreads);
        }

        public static List<ProcessThread> GetNewThreads(List<ProcessThread> threadsBefore)
        {
            var threadsAfter = GetThreadsAsList();
            // var newThreads = threadsAfter.Where(thread => !threadsBefore.Contains(thread)).ToList();
            var newThreads = GetNewThreads(threadsBefore, threadsAfter);
            return newThreads;
        }

        public static List<ProcessThread> GetNewThreads(List<ProcessThread> threadsBefore,
            List<ProcessThread> threadsAfter)
        {
            var threadIdsBefore = threadsBefore.Select(thread => thread.Id).ToList();
            var newThreads = threadsAfter.Where(thread => !threadIdsBefore.Contains(thread.Id)).ToList();
            return newThreads;
        }

        public static void ShowThreadIdAndProcessorTime(List<ProcessThread> threads)
        {
            foreach (var thread in threads)
            {
                ShowThreadIdAndProcessorTime(thread);
            }
        }

        public static void ShowThreadIdAndProcessorTime(ProcessThread processThread)
        {
            Console.WriteLine($"thread: {processThread.Id} {processThread.TotalProcessorTime}");
        }

        public static void ShowThreads(ProcessThreadCollection threads)
        {
            foreach (ProcessThread thread in threads)
            {
                Console.WriteLine($"Thread {thread}");
            }
        }

        public static void ShowThreads()
        {
            ShowThreads(GetThreads());
        }

        public static void ShowThreadCount(ProcessThreadCollection threads)
        {
            Console.WriteLine($"Thread count {threads.Count}");
        }

        public static void ShowThreadCount()
        {
            ShowThreadCount(GetThreads());
        }

        public static ProcessThreadCollection GetThreads()
        {
            var currentProcess = Process.GetCurrentProcess();
            var threads = currentProcess.Threads;
            return threads;
        }

        public static List<ProcessThread> GetThreadsAsList()
        {
            var currentProcess = Process.GetCurrentProcess();
            var threads = currentProcess.Threads;
            var output = new List<ProcessThread>();
            for (var loop = 0; loop < threads.Count; loop++)
            {
                output.Add(threads[loop]);
            }

            return output;
        }

        /// <summary>
        /// Converts a 4 byte array to a unsigned 32-bit integer with the correct endianness
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static uint BytesToUint(byte[] input)
        {
            byte[] temp;

            if (input.Length != 4)
            {
                throw new Exception("Array must be four bytes");
            }

            if (BitConverter.IsLittleEndian)
            {
                temp = new byte[4];
                Array.Copy(input, temp, 4);
                Array.Reverse(temp);
            }
            else
            {
                temp = input;
            }

            return BitConverter.ToUInt32(temp);
        }

        public static ulong GetEpochMilliseconds()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (ulong) timeSpan.TotalMilliseconds;
        }

        public static ulong GetEpochSeconds()
        {
            var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return (ulong) timeSpan.TotalSeconds;
        }

        public static void WriteHexToConsole(string name, byte[] values)
        {
            var hexValues = new List<byte>(values)
                .Select(value => $"{value:X2}")
                .ToList();
            Console.WriteLine($"{name}: {string.Join(", ", hexValues)}");
        }
    }
}