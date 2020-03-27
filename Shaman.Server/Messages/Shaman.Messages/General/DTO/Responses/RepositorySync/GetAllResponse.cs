using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.General.DTO.Responses.RepositorySync
{
    public class GetAllResponse<T> : ResponseBase
        where T:DataLightBase, new()
    {
        public int Revision { get; set; }
        public List<T> Records { get; set; }

        public GetAllResponse(ushort operationCode)
            : base(operationCode)
        {
            
        }
        
        public GetAllResponse(ushort operationCode, List<T> records, int revision)  : this(operationCode)
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