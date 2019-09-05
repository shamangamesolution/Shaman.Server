using System;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class RequestBase : MessageBase
    {
        public string EndPoint { get; set; }
        public Guid SessionId { get; set; }
        
        protected RequestBase(ushort operationCode, string endpoint = "") : base(MessageType.Request, operationCode)
        {
            this.EndPoint = endpoint;
        }

        protected override void SetMessageParameters()
        {
            IsReliable = true;
            IsOrdered = true;
            IsBroadcasted = false;
        }
        
        protected abstract void SerializeRequestBody(ISerializer serializer);
        protected abstract void DeserializeRequestBody(ISerializer serializer);
        
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.WriteBytes(SessionId.ToByteArray());
            SerializeRequestBody(serializer);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            this.SessionId = new Guid(serializer.ReadBytes());
            DeserializeRequestBody(serializer);
        }

        public string ComposeUrl(string baseUrl)
        {
            return $"{baseUrl}/{EndPoint}";
        }

        public bool IsBackendRequest()
        {
            return !string.IsNullOrEmpty(EndPoint);
        }        
    }
}