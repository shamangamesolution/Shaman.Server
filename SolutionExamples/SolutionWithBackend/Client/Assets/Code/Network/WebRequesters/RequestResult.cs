using System;

namespace Code.Network.WebRequesters
{
    public sealed class RequestResult
    {
        public byte[] Data { get; set; }
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
    }
}