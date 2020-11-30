using System;

namespace Sample.BackEnd.Helpers
{
    public class DataTableHelper
    {
        public static uint? GetNullableUint(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? (uint?)null : Convert.ToUInt32(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }
        
        public static float? GetNullableFloat(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? (float?)null : Convert.ToSingle(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }
        
        public static ushort? GetNullableUshort(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? (ushort?)null : Convert.ToUInt16(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }
    }
}