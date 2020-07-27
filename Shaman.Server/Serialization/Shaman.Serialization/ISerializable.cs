namespace Shaman.Serialization
{
    public interface ISerializable
    {        
        void Serialize(ITypeWriter typeWriter);
        void Deserialize(ITypeReader typeReader);
    }
}