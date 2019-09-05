using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.MM
{
    public class ActualizeServerResponse : ResponseBase
    {
        
        public ActualizeServerResponse() 
            : base(Messages.CustomOperationCode.ServerActualization)
        {
        }

        protected override void SerializeResponseBody(ISerializer serializer)
        {
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
        }
    }
}