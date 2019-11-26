using System;
using System.Collections.Generic;

namespace Shaman.Common.Utils.Extensions
{
    public static class DictionaryExtensions
    {

        public static bool GetBool(this Dictionary<byte, object> dict, byte key)
        {
            var val = dict.GetProperty<bool>(key);
            if (val == null)
                throw new Exception($"Property {key} was not found");
            return val.Value;
        }
        
        public static int GetInt(this Dictionary<byte, object> dict, byte key)
        {
            var val = dict.GetProperty<int>(key);
            if (val == null)
                throw new Exception($"Property {key} was not found");
            return val.Value;
        }
        public static float GetFloat(this Dictionary<byte, object> dict, byte key)
        {
            var val = dict.GetProperty<float>(key);
            if (val == null)
                throw new Exception($"Property {key} was not found");
            return val.Value;
        }
        public static byte GetByte(this Dictionary<byte, object> dict, byte key)
        {
            var val = dict.GetProperty<byte>(key);
            if (val == null)
                throw new Exception($"Property {key} was not found");
            return val.Value;
        }
        
        public static T? GetProperty<T>(this Dictionary<byte, object> dict, byte key)
            where T : struct 
        {
            if (!dict.TryGetValue(key, out var val))
                return null;
            else
            {
                return (T?)val;
            }
            
        }
                
        public static bool GetProperty<T>(this Dictionary<byte, object> dict, byte key, out T? value)
            where T : struct
        {
            value = null;
            if (!dict.TryGetValue(key, out var val))
                return false;
            else
            {
                value = (T?) val;
                return true;
            }
            
        }

    }
}