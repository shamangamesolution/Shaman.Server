using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.Extensions
{
    public static class BaseTypesSerializerExtensions
    {
        public static void Write(this ITypeWriter typeWriter, byte? value)
        {
            if (value == null)
                typeWriter.Write(false);
            else
            {
                typeWriter.Write(true);
                typeWriter.Write(value.Value);
            }
        }
        
        public static byte? ReadNullableByte(this ITypeReader typeWriter)
        {
            var notIsNull = typeWriter.ReadBool();
            if (notIsNull)
                return typeWriter.ReadByte();

            return null;
        }
    }
}