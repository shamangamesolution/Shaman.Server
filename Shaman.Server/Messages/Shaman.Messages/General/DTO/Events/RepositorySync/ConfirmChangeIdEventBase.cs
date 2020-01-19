using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events.RepositorySync
{
    public class ConfirmChangeIdEventBase : EventBase
    {
        public int ChangeId { get; set; }
        
        public ConfirmChangeIdEventBase(ushort operationCode, int changeId) : this(operationCode)
        {
            ChangeId = changeId;
        }
        
        
        public ConfirmChangeIdEventBase(ushort operationCode) : base(operationCode)
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