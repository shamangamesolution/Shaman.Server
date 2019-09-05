using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.Messages.General.DTO.Responses.Router
{
    public class GetBackendsListResponse : ResponseBase
    {
        public List<Backend> Backends { get; set; }
        
        public GetBackendsListResponse(List<Backend> backends) : base(Shaman.Common.Utils.Messages.OperationCode.GetBackendsList)
        {
            Backends = backends;
        }

        public GetBackendsListResponse() : base(Shaman.Common.Utils.Messages.OperationCode.GetBackendsList)
        {
            Backends = new List<Backend>();
        }

        protected override void SerializeResponseBody(ISerializer serializer)
        {
            serializer.WriteList(Backends);
        }

        protected override void DeserializeResponseBody(ISerializer serializer)
        {
            Backends = serializer.ReadList<Backend>();
        }
    }
}