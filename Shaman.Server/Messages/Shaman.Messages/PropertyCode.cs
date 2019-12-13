namespace Shaman.Messages
{
    public struct PropertyCode
    {

        public struct PlayerProperties
        {
            public const byte League = 2;
            public const byte PlayerPower = 3;
            public const byte Level = 11;
            public const byte BackendId = 16;
            public const byte GameMode = 20;
        }

        public struct RoomProperties
        {
            public const byte GameMode = 1;
            public const byte TotalPlayersNeeded = 4;
            public const byte MatchMakingTick = 5;
            public const byte MaximumMmTime = 8;
        }
    }
}