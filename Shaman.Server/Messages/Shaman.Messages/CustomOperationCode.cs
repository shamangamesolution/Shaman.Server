namespace Shaman.Messages
{
    public struct CustomOperationCode
    {
        public const ushort Authorization = 10001;
        public const ushort Ping = 10002;
        public const ushort JoinRoom = 10003;        
        public const ushort JoinRoomResponse = 10004;        
        public const ushort Test = 10006;
        public const ushort LeaveRoom = 10009;
        public const ushort Disconnect = 10010;
        public const ushort Connect = 10011;
        public const ushort PingRequest = 10012;
        public const ushort Error = 10000;
        public const ushort PingResponse = 10013;
        public const ushort AuthorizationResponse = 10014;


        //matchmaking.BEGIN
        public const ushort EnterMatchMaking = 11012;
        public const ushort LeaveMatchMaking = 11013;
        public const ushort CreateRoomFromMm = 11014;
        public const ushort JoinInfo = 11015;
        public const ushort UpdateRoom = 11018;
        public const ushort GetRoomList = 11019;
        public const ushort DirectJoin = 11020;
        public const ushort UpdateRoomState = 11021;
        public const ushort CreateRoomFromClient = 11022;
        public const ushort CreateRoomFromClientResponse = 11023;
        public const ushort DirectJoinResponse = 11024;
        public const ushort EnterMatchMakingResponse = 11025;
        public const ushort GetRoomListResponse = 11026;
        public const ushort LeaveMatchMakingResponse = 11027;
        
        //matchmaking.END
        
        //router.BEGIN
        public const ushort GetServerInfoList = 12003;
        //router.END
        
        //backend.BEGIN
        public const ushort GetSessionId = 13001;
        public const ushort GetSessionIdResponse = 13002;
        public const ushort IsOnServiceHttp = 15057;
        public const ushort GetAuthToken = 15060;


        public const ushort ValidateSessionId = 15109;
        //backend.END
        
        //RW.operations.BEGIN
        public const ushort ActualizeServer =  16096;
        //RW.operations.END

    }
}