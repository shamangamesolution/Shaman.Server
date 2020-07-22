namespace Shaman.Common.Contract
{
    public struct Payload
    {
        public readonly byte[] Buffer;
        public readonly int Offset;
        public readonly int Length;

        public Payload(byte[] buffer, int offset, int length)
        {
            Buffer = buffer;
            Offset = offset;
            Length = length;
        }
    }
}