using System;

namespace Shaman.Client.Peers
{
    public class ShamanClientException : Exception
    {
        public ShamanClientException(string message) : base(message)
        {
        }

        public ShamanClientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}