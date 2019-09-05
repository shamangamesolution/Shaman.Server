namespace Shaman.Messages
{
    public struct PropertyCode
    {

        public struct PlayerProperties
        {
            public const byte Level = 11;
            public const byte IsBot = 15;
            public const byte BackendId = 16;
            public const byte GameMode = 20;
        }

        public struct RoomProperties
        {
            public const byte GameMode = 1;
        }
    }
}