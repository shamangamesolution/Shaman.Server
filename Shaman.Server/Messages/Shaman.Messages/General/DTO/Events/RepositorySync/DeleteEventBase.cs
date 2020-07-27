using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.General.DTO.Events.RepositorySync
{
    public abstract class DeleteEventBase : EventBase 
    {
        public override bool IsReliable => true;
        public List<DeletedInfo> DeletedInfoList { get; set; }
        public int ChangeId { get; set; }
        
        public DeleteEventBase(byte operationCode, List<DeletedInfo> deletedInfoList, int changeId)
            :this(operationCode)
        {
            DeletedInfoList = deletedInfoList;
            ChangeId = changeId;

        }
        
        public DeleteEventBase(byte operationCode) : base(operationCode)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteList(DeletedInfoList);
            typeWriter.Write(ChangeId);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            DeletedInfoList = typeReader.ReadList<DeletedInfo>();
            ChangeId = typeReader.ReadInt();
        }
    }
}