using System;
using System.Runtime.Serialization;

namespace Shaman.Common.Utils.Exceptions
{
    public enum StoreExceptionCode
    {
        OK,
        NotEnoughSc,
        NotEnoughHc,
        DalException,
        GeneralException,
        RouletteFailed,
        VendotTokenIsInvalid,
        PriceNotFound
    }

    public class StoreException : Exception
    {
        public StoreExceptionCode Code;

        public string GetString()
        {
            switch (Code)
            {
                case StoreExceptionCode.NotEnoughSc:
                    return "Not enough silver";
                case StoreExceptionCode.NotEnoughHc:
                    return "Not enough gold";
                default:
                    return this.Message;
            }
        }

        public StoreException(StoreExceptionCode code)
        {
            Code = code;
        }

        public StoreException(StoreExceptionCode code, string message) : base(message)
        {
            Code = code;
        }

        public StoreException(StoreExceptionCode code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        protected StoreException(StoreExceptionCode code, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Code = code;
        }
    }
}
