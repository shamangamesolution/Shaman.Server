namespace Shaman.Game.Contract
{
    public struct SendOptions
    {
        public bool IsReliable;
        public bool IsOrdered;

        public SendOptions(bool isReliable, bool isOrdered)
        {
            IsReliable = isReliable;
            IsOrdered = isOrdered;
        }
    }
}