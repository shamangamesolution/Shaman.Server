using System;

namespace Shaman.DAL.Exceptions
{
    public class SqlDalException: Exception
    {
        public SqlDalException()
        {
        }

        public SqlDalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}