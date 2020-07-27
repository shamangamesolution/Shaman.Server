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
    public class InsertEventBase<T> : EventBase where T:DataLightBase, new()
    {
        public override bool IsReliable => true;
        public List<InsertedInfo<T>> InsertedInfoList { get; set; }
        public int ChangeId { get; set; }

        public InsertEventBase(byte operationCode, List<InsertedInfo<T>> insertedInfoList, int changeId)
            :this(operationCode)
        {
            InsertedInfoList = insertedInfoList;
            ChangeId = changeId;
        }
        
        public InsertEventBase(byte operationCode) : base(operationCode)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteList(InsertedInfoList);
            typeWriter.Write(ChangeId);

        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            InsertedInfoList = typeReader.ReadList<InsertedInfo<T>>();
            ChangeId = typeReader.ReadInt();

        }
    }
}