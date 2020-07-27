using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Serialization.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Messages;
using Shaman.Serialization.Messages.Udp;

namespace Shaman.Messages.General.DTO.Requests.RepositorySync
{
    public class GetAllRequestBase : RequestBase
    {
        public GetAllRequestBase(byte operationCode) : base(operationCode)
        {
        }

        protected override void SerializeRequestBody(ITypeWriter typeWriter)
        {
        }

        protected override void DeserializeRequestBody(ITypeReader typeReader)
        {
        }
    }
}