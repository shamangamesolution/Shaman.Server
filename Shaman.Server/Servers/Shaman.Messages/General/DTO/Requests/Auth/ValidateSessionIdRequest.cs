﻿using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.DTO.Requests.Auth
{
    public class ValidateSessionIdRequest : RequestBase
    {
        public string Secret { get; set; }
        public ValidateSessionIdRequest()
            :base(CustomOperationCode.ValidateSessionId, BackEndEndpoints.ValidateSessionId)
        {

        }

        public ValidateSessionIdRequest(Guid token, string secret)
            :this()
        {
            this.SessionId = token;
            this.Secret = secret;
        }
        
        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.Write(Secret);
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            Secret = serializer.ReadString();
        }
    }
}
