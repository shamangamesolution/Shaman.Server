using System;

namespace Shaman.Serialization.Extensions
{
    public static class BaseTypesSerializationExtensions
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

        public static void Write(this ITypeWriter writer, DateTime? dateTime)
        {
            writer.Write(dateTime?.ToBinary() ?? 0L);
        }

        public static void Write(this ITypeWriter writer, TimeSpan? timeSpan)
        {
            writer.Write(timeSpan?.Ticks ?? 0L);
        }

        public static void Write(this ITypeWriter writer, Guid? guid)
        {
            if (guid.HasValue)
            {
                writer.Write((byte) 1);
                writer.Write(guid.Value);
            }
            else
            {
                writer.Write((byte) 0);
            }
        }

        public static void WriteNullable<T>(this ITypeWriter writer, T obj) where T : class, ISerializable
        {
            if (obj != null)
            {
                writer.Write(true);
                writer.Write(obj);
            }
            else
            {
                writer.Write(false);
            }
        }

        public static void Write(this ITypeWriter typeWriter, ISerializable data)
        {
            data.Serialize(typeWriter);
        }

        public static T Read<T>(this ITypeReader typeReader) where T:ISerializable, new()
        {
            var data = new T(); 
            data.Deserialize(typeReader);
            return data;
        }

        public static Guid? ReadNullableGuid(this ITypeReader reader)
        {
            return reader.ReadByte() != 0 ? reader.ReadGuid() : (Guid?) null;
        }

        public static DateTime? ReadNullableDate(this ITypeReader reader)
        {
            var dateData = reader.ReadLong();
            if (dateData == 0)
            {
                return null;
            }

            return DateTime.FromBinary(dateData);
        }

        public static T ReadNullable<T>(this ITypeReader reader) where T : class, ISerializable, new()
        {
            return reader.ReadBool() ? reader.Read<T>() : default;
        }
    }
}