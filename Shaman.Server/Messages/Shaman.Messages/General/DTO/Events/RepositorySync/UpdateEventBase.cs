using System.Collections.Generic;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Messages.General.Entity;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.General.DTO.Events.RepositorySync
{
    public class UpdateEventBase : EventBase
    {
        public override bool IsReliable => true;
        public List<UpdatedInfo> UpdatedInfoList { get; set; }
        public int ChangeId { get; set; }

        public UpdateEventBase(byte operationCode, List<UpdatedInfo> updatedInfoList, int changeId)
            :this(operationCode)
        {
            UpdatedInfoList = updatedInfoList;
            ChangeId = changeId;
        }
        
        public UpdateEventBase(byte operationCode) : base(operationCode)
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