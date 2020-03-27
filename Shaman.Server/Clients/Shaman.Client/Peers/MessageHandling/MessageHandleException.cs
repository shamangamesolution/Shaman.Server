using System;

namespace Shaman.Client.Peers.MessageHandling
{
    public class MessageHandleException : Exception
    {
        public MessageHandleException(string message) : base(message)
        {
        }

        public MessageHandleException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}