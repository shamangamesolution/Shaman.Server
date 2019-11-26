namespace Shaman.Game.Contract
{
    public struct MessageData
    {
        public byte[] Buffer { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
    }
}