using System;
using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public abstract class RequestBase : MessageBase
    {
        public override bool IsReliable => true;
        public override bool IsBroadcasted => false;

        
        public string EndPoint { get; set; }
        public Guid SessionId { get; set; }
        public List<TransitItem> TransitItems { get; set; }

        protected RequestBase(ushort operationCode, string endpoint = "") : base(MessageType.Request, operationCode)
        {
            this.EndPoint = endpoint;
        }
        
        protected abstract void SerializeRequestBody(ITypeWriter typeWriter);
        protected abstract void DeserializeRequestBody(ITypeReader typeReader);
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(SessionId);
            typeWriter.WriteList(TransitItems);
            SerializeRequestBody(typeWriter);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            this.SessionId = typeReader.ReadGuid();
            this.TransitItems = typeReader.ReadList<TransitItem>();
            DeserializeRequestBody(typeReader);
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