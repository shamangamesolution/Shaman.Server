namespace Shaman.Messages
{
    public struct CustomOperationCode
    {
        public const ushort Authorization = 10001;
        public const ushort Ping = 10002;
        public const ushort JoinRoom = 10003;        
        public const ushort Test = 10006;
        public const ushort LeaveRoom = 10009;
        public const ushort Disconnect = 10010;
        public const ushort Connect = 10011;
        public const ushort PingRequest = 10012;


        //matchmaking.BEGIN
        public const ushort EnterMatchMaking = 11012;
        public const ushort LeaveMatchMaking = 11013;
        public const ushort CreateRoom = 11014;
        public const ushort JoinInfo = 11015;
        public const ushort ServerActualization = 11017;
        //matchmaking.END
        
        //router.BEGIN
        public const ushort ActualizeMatchmaker = 12002;
        //router.END
        
        //backend.BEGIN
        public const ushort GetSessionId = 13001;
        
        public const ushort Initialization = 15001;
        public const ushort GetCurrentStorageVersion = 15003;
        public const ushort GetStorageHttp = 15056;
        public const ushort IsOnServiceHttp = 15057;
        public const ushort GetAuthToken = 15060;


        public const ushort ValidateSessionId = 15109;
        public const ushort GetMatchmakers = 15110;
        public const ushort GetNotCompressedStorage = 15111;

        //backend.END
    }
}