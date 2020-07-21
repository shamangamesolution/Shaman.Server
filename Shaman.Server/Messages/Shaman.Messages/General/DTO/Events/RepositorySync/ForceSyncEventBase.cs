using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Events.RepositorySync
{
    public class ForceSyncEventBase : EventBase
    {
        public override bool IsReliable => true;

        public ForceSyncEventBase(byte operationCode) : base(operationCode)
        {
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
        }
    }
}