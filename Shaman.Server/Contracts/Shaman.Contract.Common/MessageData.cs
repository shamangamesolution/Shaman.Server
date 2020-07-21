namespace Shaman.Common.Contract
{
    public struct MessageData
    {
        public readonly byte[] Buffer;
        public readonly int Offset;
        public readonly int Length;

        public MessageData(byte[] buffer, int offset, int length)
        {
            Buffer = buffer;
            Offset = offset;
            Length = length;
        }
    }
}