using Amazon.Runtime.EventStreams.Internal;
using System;

namespace GGCSharp
{
    public class GreengrassEventStreamException : EventStreamException
    {
        public GreengrassEventStreamException()
        {
        }

        public GreengrassEventStreamException(string message)
            : base(message)
        {
        }

        public GreengrassEventStreamException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}