using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.General.DTO.Events.RepositorySync
{
    public class UpdateEventBase : EventBase
    {
        public override bool IsReliable => true;
        public List<UpdatedInfo> UpdatedInfoList { get; set; }
        public int ChangeId { get; set; }

        public UpdateEventBase(ushort operationCode, List<UpdatedInfo> updatedInfoList, int changeId)
            :this(operationCode)
        {
            UpdatedInfoList = updatedInfoList;
            ChangeId = changeId;
        }
        
        public UpdateEventBase(ushort operationCode) : base(operationCode)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteList(UpdatedInfoList);
            typeWriter.Write(ChangeId);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            UpdatedInfoList = typeReader.ReadList<UpdatedInfo>();
            ChangeId = typeReader.ReadInt();
        }
    }
}