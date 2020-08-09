using System;
using System.Collections.Generic;
using System.Data;
using Shaman.Contract.Common.Logging;
using Shaman.DAL.MySQL;

namespace Shaman.DAL.Repositories
{
    public class RepositoryBase
    {
        protected SqlDal dal;

        protected string DbServer, DbName, DbUser, DbPassword;        

        private IShamanLogger Logger;       
        
        public void Initialize(string dbServer, string dbName, string dbUser, string dbPassword, int maxPoolSize, IShamanLogger logger)
        {
            Logger = logger;
            DbServer = dbServer;
            DbName = dbName;
            DbUser = dbUser;
            DbPassword = dbPassword;
            
           
            //init dal
            dal = new SqlDal(DbServer, DbName, DbUser, DbPassword, maxPoolSize, (s) =>
            {
                Logger.Error("DAL", "dal", s);
            });
        }

        protected int? GetId(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return null;

            return GetNullableInt(dt.Rows[0]["id"]);
        }
        
        protected void LogInfo(string source, string message)
        {
            Logger.Info("DAL", source, message);
        }

        protected void LogWarning(string source, string message)
        {
            Logger.Warning("DAL", source, message);
        }
        
        protected void LogError(string source, string message)
        {
            Logger.Error("DAL", source, message);
        }
        
        protected List<int> GetIdList(DataTable dt)
        {
            var result = new List<int>();

            if (dt == null || dt.Rows.Count == 0)
                return result;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                result.Add(GetInt(dt.Rows[i]["id"]));
            }

            return result;
        }

        protected int GetMySQLTinyInt(Boolean value)
        {
            if (value)
                return 1;
            else
                return 0;
        }

        /// <summary>
        /// prepares text value for query - null/not null
        /// </summary>
        protected string Value(object obj)
        {
            if (obj == null)
                return "null";
            else
            {
                //mysql datetime
                if (obj is DateTime)
                    return ((DateTime)obj).ToString("yyyyMMddHHmmss");
                else
                    if (obj is DateTime?)
                        return ((DateTime?)obj).Value.ToString("yyyyMMddHHmmss");
                    else
                        return $"'{obj.ToString()}'";
            }
        }

        protected int GetInt(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? 0 : Convert.ToInt32(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }

        protected uint GetUInt(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? 0 : Convert.ToUInt32(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }
        
        protected ulong GetULong(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? 0 : Convert.ToUInt64(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }
        
        protected int? GetNullableInt(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? (int?)null : Convert.ToInt32(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }
        
        protected float? GetNullableFloat(object obj)
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

        protected byte? GetNullableByte(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? (byte?)null : Convert.ToByte(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }

        protected short GetShort(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? (short)0 : Convert.ToInt16(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }

        protected ushort GetUshort(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? (ushort)0 : (ushort)Convert.ToUInt16(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }

        protected byte GetByte(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? (byte)0 : Convert.ToByte(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }

        protected float GetFloat(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? 0 : Convert.ToSingle(obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting {obj} field", ex);
            }
        }

        protected string GetString(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? "" : Convert.ToString(obj);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error converting {obj} field", ex);
            }

        }

        protected Boolean GetBoolean(object obj, bool defaultValue = false)
        {
            try
            {
                return (obj == null || obj is DBNull) ? defaultValue : Convert.ToBoolean(obj);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error converting {obj} field", ex);
            }

        }

        protected DateTime? GetNullableDateTime(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? null : (DateTime?)Convert.ToDateTime(obj);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error converting {obj} field", ex);
            }

        }

        protected DateTime GetDateTime(object obj)
        {
            try
            {
                return (obj == null || obj is DBNull) ? DateTime.MinValue : Convert.ToDateTime(obj);
            }
            catch (Exception ex)
            {

                throw new Exception($"Error converting {obj} field", ex);
            }

        }

        protected string ClearStringData(string inputString)
        {
            return inputString.Replace("'", "").Replace("`", "").Replace(" ", "");
        }
    }
}
