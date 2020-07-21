namespace Shaman.Common.Contract
{
    public struct DeliveryOptions
    {
        public bool IsReliable;
        public bool IsOrdered;

        public DeliveryOptions(bool isReliable, bool isOrdered)
        {
            IsReliable = isReliable;
            IsOrdered = isOrdered;
        }
    }
}