using System;
using Shaman.Messages;
using Shaman.Serialization;
using Shaman.Serialization.Extensions;

namespace Shaman.Client
{
    public class BundleMessageWrapper<TBundleMessage> : ISerializable where TBundleMessage:ISerializable
    {
        private readonly TBundleMessage _innerMessage;

        public BundleMessageWrapper(TBundleMessage innerMessage)
        {
            _innerMessage = innerMessage;
        }

        public void Serialize(ITypeWriter typeWriter)
        {
            typeWriter.Write(ShamanOperationCode.Bundle);
            typeWriter.Write(_innerMessage);
        }

        public void Deserialize(ITypeReader typeReader)
        {
            throw new NotImplementedException("Should not be used in deserialization process");
        }
    }
}