using System;
using System.Runtime.Serialization;

namespace Shaman.Common.Utils.Exceptions
{
    public enum UpgradeExceptionCode
    {
        MaxLevelReached,
        GeneralException,
        PlayerWasNotSet,
        PlayerRobotCollectionIsEmpty,
        PlayerRobotWasNotFound,
        NoUpgradeForThisParameter
    }

    public class UpgradeException : Exception
    {
        public UpgradeExceptionCode Code;

        public string GetString()
        {
            switch(Code)
            {
                default:
                    return this.Message;
            }
        }

        public UpgradeException(UpgradeExceptionCode code)
        {
            Code = code;
        }

        public UpgradeException(UpgradeExceptionCode code, string message) : base(message)
        {
            Code = code;
        }

        public UpgradeException(UpgradeExceptionCode code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        protected UpgradeException(UpgradeExceptionCode code, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Code = code;
        }
    }
}
