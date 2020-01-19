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
        
        public static void Write(this ITypeWriter typeWriter, int? value)
        {
            if (value == null)
                typeWriter.Write(false);
            else
            {
                typeWriter.Write(true);
                typeWriter.Write(value.Value);
            }
        }
        
        public static int? ReadNullableInt(this ITypeReader typeReader)
        {
            var notIsNull = typeReader.ReadBool();
            if (notIsNull)
                return typeReader.ReadInt();

            return null;
        }
        
        public static void Write(this ITypeWriter typeWriter, float? value)
        {
            if (value == null)
                typeWriter.Write(false);
            else
            {
                typeWriter.Write(true);
                typeWriter.Write(value.Value);
            }
        }
        
        public static float? ReadNullableFloat(this ITypeReader typeReader)
        {
            var notIsNull = typeReader.ReadBool();
            if (notIsNull)
                return typeReader.ReadFloat();

            return null;
        }
    }
}