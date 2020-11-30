using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public interface ISerializable
    {        
        void Serialize(ITypeWriter typeWriter);
        void Deserialize(ITypeReader typeReader);
    }
}