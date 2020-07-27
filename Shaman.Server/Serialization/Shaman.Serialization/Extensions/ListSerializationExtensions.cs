using System.Collections.Generic;
using System.Linq;

namespace Shaman.Serialization.Extensions
{
    public static class ListSerializationExtensions
    {
        public static List<T> ReadList<T>(this ITypeReader reader)
            where T : ISerializable, new()
        {
            var result = new List<T>();

            var length = reader.ReadInt();

            if (length != 0)
            {
                for (var i = 0; i < length; i++)
                {
                    var item = new T();
                    item.Deserialize(reader);
                    result.Add(item);
                }
            }

            return result;
        }

        public static void WriteList<T>(this ITypeWriter bw, ICollection<T> list)
            where T : ISerializable
        {
            if (list != null)
            {
                bw.Write(list.Count);
                foreach (var el in list)
                {
                    el.Serialize(bw);
                }
            }
            else
            {
                bw.Write(0);
            }
        }

        public static void WriteList(this ITypeWriter bw, IList<int> list)
        {
            if (list != null && list.Any())
            {
                bw.Write(list.Count);
                for (var i = 0; i < list.Count; i++)
                {
                    bw.Write(list[i]);
                }
            }
            else
            {
                bw.Write(0);
            }
        }

        public static void WriteList(this ITypeWriter bw, IList<string> list)
        {
            if (list != null && list.Any())
            {
                bw.Write(list.Count);
                for (var i = 0; i < list.Count; i++)
                {
                    bw.Write(list[i]);
                }
            }
            else
            {
                bw.Write(0);
            }
        }

        public static List<int> ReadListOfInt(this ITypeReader reader)
        {
            var list = new List<int>();
            var listLength = reader.ReadInt();
            if (listLength != 0)
            {
                for (int i = 0; i < listLength; i++)
                {
                    list.Add(reader.ReadInt());
                }
            }

            return list;
        }

        public static List<string> ReadListOfString(this ITypeReader reader)
        {
            var list = new List<string>();
            var listLength = reader.ReadInt();
            if (listLength != 0)
            {
                for (int i = 0; i < listLength; i++)
                {
                    list.Add(reader.ReadString());
                }
            }

            return list;
        }

        public static List<byte> ReadListOfByte(this ITypeReader reader)
        {
            var list = new List<byte>();
            var listLength = reader.ReadInt();
            if (listLength != 0)
            {
                for (int i = 0; i < listLength; i++)
                {
                    list.Add(reader.ReadByte());
                }
            }

            return list;
        }
    }
}