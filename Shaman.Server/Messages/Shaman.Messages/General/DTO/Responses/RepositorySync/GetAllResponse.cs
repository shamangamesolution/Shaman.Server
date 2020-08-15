using System.Collections.Generic;
using Shaman.Messages.General.Entity;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.General.DTO.Responses.RepositorySync
{
    public class GetAllResponse<T> : ResponseBase
        where T:DataLightBase, new()
    {
        public int Revision { get; set; }
        public List<T> Records { get; set; }

        public GetAllResponse(byte operationCode)
            : base(operationCode)
        {
            
        }
        
        public GetAllResponse(byte operationCode, List<T> records, int revision)  : this(operationCode)
        {
            Records = records;
            Revision = revision;
        }

        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteList(Records);
            typeWriter.Write(Revision);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            Records = typeReader.ReadList<T>();
            Revision = typeReader.ReadInt();
        }
    }
}