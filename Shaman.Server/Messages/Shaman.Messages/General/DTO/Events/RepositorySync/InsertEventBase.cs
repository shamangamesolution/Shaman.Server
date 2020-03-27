using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.General.DTO.Events.RepositorySync
{
    public class InsertEventBase<T> : EventBase where T:DataLightBase, new()
    {
        public override bool IsReliable => true;
        public List<InsertedInfo<T>> InsertedInfoList { get; set; }
        public int ChangeId { get; set; }

        public InsertEventBase(ushort operationCode, List<InsertedInfo<T>> insertedInfoList, int changeId)
            :this(operationCode)
        {
            InsertedInfoList = insertedInfoList;
            ChangeId = changeId;
        }
        
        public InsertEventBase(ushort operationCode) : base(operationCode)
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