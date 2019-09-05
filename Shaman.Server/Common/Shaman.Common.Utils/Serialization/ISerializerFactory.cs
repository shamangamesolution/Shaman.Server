
namespace Shaman.Common.Utils.Serialization
{
    public interface ISerializerFactory
    {
        void InitializeDefaultSerializers(int minLen, string source);
        ISerializer GetSimpleSerializer();
        ISerializer GetStrictSerializer();
        ISerializer GetBinarySerializer();
    }
}