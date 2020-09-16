namespace Shaman.TestTools.Events
{
    public struct FakePropertyCodes
    {
        public struct PlayerProperties
        {
            public static byte GameMode = 1;
            public static byte Level = 2;
            public static byte BackendId = 3;
        } 
        public struct RoomProperties
        {
            public const byte GameMode = 1;
            public const byte TotalPlayersNeeded = 4;
            public const byte MatchMakingTick = 5;
            public const byte MaximumMmTime = 8;
            public const byte MatchMakerUrl = 9;
        }
    }
}