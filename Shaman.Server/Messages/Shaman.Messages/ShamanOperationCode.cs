namespace Shaman.Messages
{
    public struct ShamanOperationCode
    {
        public const byte Error = 0;
        public const byte Authorization = 1;
        public const byte Ping = 2;
        public const byte JoinRoom = 3;
        public const byte JoinRoomResponse = 4;
        public const byte LeaveRoom = 5;
        public const byte Disconnect = 6;
        public const byte Connect = 7;
        public const byte PingRequest = 8;
        public const byte PingResponse = 9;
        public const byte AuthorizationResponse = 10;
        //matchmaking.BEGIN
        public const byte EnterMatchMaking = 11;
        public const byte LeaveMatchMaking = 12;
        public const byte JoinInfo = 13;
        public const byte GetRoomList = 14;
        public const byte DirectJoin = 15;
        public const byte CreateRoomFromClient = 16;
        public const byte CreateRoomFromClientResponse = 17;
        public const byte DirectJoinResponse = 18;
        public const byte EnterMatchMakingResponse = 19;
        public const byte GetRoomListResponse = 20;
        public const byte LeaveMatchMakingResponse = 21;
        public const byte JoinRandomRoom = 22;
        public const byte JoinRandomRoomResponse = 23;
        //matchmaking.END
        
        /// <summary>
        /// Bundle message
        /// </summary>
        public const byte Bundle = 254;
        
        public const byte _ = 255;// to extend one more byte if need
    }
}