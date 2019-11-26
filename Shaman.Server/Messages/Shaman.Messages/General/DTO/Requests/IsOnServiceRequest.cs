﻿using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests
{
    public class IsOnServiceRequest : RequestBase
    {
        public IsOnServiceRequest()
            :base(CustomOperationCode.IsOnServiceHttp, BackEndEndpoints.IsOnService)
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
