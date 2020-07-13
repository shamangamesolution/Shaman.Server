using System;
using System.Collections.Generic;
using System.Data;
using Shaman.DAL.MySQL;

namespace Shaman.DAL.Repositories
{
    public class RepositoryBase
    {
        protected readonly ISqlDal Dal;

        protected RepositoryBase(ISqlDal dal)
        {
            Dal = dal;
        }

        protected int? GetId(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return null;

            return GetNullableInt(dt.Rows[0]["id"]);
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


        protected int GetInt(object obj)
        {
            return Convert.ToInt32(obj);
        }

        protected uint GetUInt(object obj)
        {
            return Convert.ToUInt32(obj);
        }

        protected ulong GetULong(object obj)
        {
            return Convert.ToUInt64(obj);
        }

        protected int? GetNullableInt(object obj)
        {
            return (obj == null || obj is DBNull) ? (int?) null : Convert.ToInt32(obj);
        }

        protected float? GetNullableFloat(object obj)
        {
            return (obj == null || obj is DBNull) ? (float?) null : Convert.ToSingle(obj);
        }

        protected byte? GetNullableByte(object obj)
        {
            return (obj == null || obj is DBNull) ? (byte?) null : Convert.ToByte(obj);
        }

        protected short GetShort(object obj)
        {
            return Convert.ToInt16(obj);
        }

        protected ushort GetUshort(object obj)
        {
            return Convert.ToUInt16(obj);
        }

        protected byte GetByte(object obj)
        {
            return Convert.ToByte(obj);
        }

        protected float GetFloat(object obj)
        {
            return Convert.ToSingle(obj);
        }

        protected string GetString(object obj)
        {
            return Convert.ToString(obj);
        }

        protected Boolean GetBoolean(object obj)
        {
            return Convert.ToBoolean(obj);
        }

        protected DateTime? GetNullableDateTime(object obj)
        {
            return (obj == null || obj is DBNull) ? null : (DateTime?) Convert.ToDateTime(obj);
        }

        protected DateTime GetDateTime(object obj)
        {
            return Convert.ToDateTime(obj);
        }
    }
}