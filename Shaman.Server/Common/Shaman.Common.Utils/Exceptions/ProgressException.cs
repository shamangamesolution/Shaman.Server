using System;
using System.Runtime.Serialization;

namespace Shaman.Common.Utils.Exceptions
{
    public enum ProgressExceptionCode
    {
        BoxIsNotAvailableForOpen,
        NoAdWatchesAvailable,
        PlayerNameIsNotUnique,
        ConsumableIsNotAvailableToBuy,
        GeneralException
    }

    public class ProgressException : Exception
    {
        public ProgressExceptionCode Code;

        public string GetString()
        {
            switch (Code)
            {
                case ProgressExceptionCode.NoAdWatchesAvailable:
                    return "No Ad watches available for now";
                case ProgressExceptionCode.PlayerNameIsNotUnique:
                    return "Player name is not unique!";
                default:
                    return this.Message;
            }
        }

        public ProgressException(ProgressExceptionCode code)
            : base(code.ToString())
        {
            Code = code;
        }

        public ProgressException(ProgressExceptionCode code, string message) : base(message)
        {
            Code = code;
        }

        public ProgressException(ProgressExceptionCode code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        protected ProgressException(ProgressExceptionCode code, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Code = code;
        }
    }
}
