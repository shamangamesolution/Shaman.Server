using System;
using System.Runtime.Serialization;

namespace Shaman.DAL.Exceptions
{
    public enum DalExceptionCode
    {
        PlayerNotFound,
        GeneralException,
        VersionWasNotFound,
        ExternalAccountWasNotFound,
        PlayerRobotNotFound,
        UpgradeWasNotFound,
        ShopItemWasNotFound,
        PackageWasNotFound,
        MedalsWereNotFound,
        MatchMakingNotFound
    }

    public class DalException : Exception
    {
        public DalExceptionCode Code;

        public DalException(DalExceptionCode code)
        {
            Code = code;
        }

        public DalException(DalExceptionCode code, string message) : base(message)
        {
            Code = code;
        }

        public DalException(DalExceptionCode code, string message, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

        protected DalException(DalExceptionCode code, SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Code = code;
        }
    }
}
