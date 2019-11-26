using System;
using System.Runtime.Serialization;

namespace Shaman.Common.Utils.Exceptions
{
    public enum StoreExceptionCode
    {
        OK,
        NotEnoughSilver,
        NotEnoughGold,
        NotEnoughPoints,
        DalException,
        GeneralException,
        RouletteFailed,
        VendotTokenIsInvalid,
        MaxRentReached,
        MaxSimultaneousUpgradesReached,
        MaxBoxesReached,
        NotEnoughShards,
        NotEnoughGoldForExchange,
        DuplicateTransaction,
        NotEnoughCurrency
    }

    public class StoreException : Exception
    {
        public StoreExceptionCode Code;

        public string GetString()
        {
            switch (Code)
            {
                case StoreExceptionCode.NotEnoughSilver:
                    return "Not enough silver";
                case StoreExceptionCode.NotEnoughGold:
                    return "Not enough gold";
                case StoreExceptionCode.NotEnoughPoints:
                    return "Not enough points";
                case StoreExceptionCode.DuplicateTransaction:
                    return "Duplicate transaction";
                default:
                    return this.Message;
            }
        }

        public StoreException(StoreExceptionCode code)
            :base(code.ToString())
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
