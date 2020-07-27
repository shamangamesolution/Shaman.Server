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
        public Payload(params byte[] buffer)
        {
            Buffer = buffer;
            Offset = 0;
            Length = buffer.Length;
        }
    }
}