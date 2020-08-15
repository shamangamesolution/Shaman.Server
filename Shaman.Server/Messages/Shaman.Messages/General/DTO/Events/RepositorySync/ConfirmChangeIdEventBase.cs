using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.General.DTO.Events.RepositorySync
{
    public class ConfirmChangeIdEventBase : EventBase
    {
        public override bool IsReliable => false;
        public int ChangeId { get; set; }
        
        public ConfirmChangeIdEventBase(byte operationCode, int changeId) : this(operationCode)
        {
            ChangeId = changeId;
        }
        
        
        public ConfirmChangeIdEventBase(byte operationCode) : base(operationCode)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(ChangeId);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            ChangeId = typeReader.ReadInt();
        }
    }
}