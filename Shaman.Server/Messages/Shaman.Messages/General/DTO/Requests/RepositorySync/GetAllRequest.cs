using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

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