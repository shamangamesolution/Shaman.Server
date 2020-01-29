namespace Shaman.Game.Contract
{
    public struct MessageData
    {
        public readonly byte[] Buffer;
        public readonly int Offset;
        public readonly int Length;
        public readonly bool IsReliable;

        public MessageData(byte[] buffer, int offset, int length, bool isReliable)
        {
            Buffer = buffer;
            Offset = offset;
            Length = length;
            IsReliable = isReliable;
        }
    }
}