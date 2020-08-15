namespace Shaman.Contract.Common
{
    public readonly struct DeliveryOptions
    {
        public readonly bool IsReliable;
        public readonly bool IsOrdered;

        public DeliveryOptions(bool isReliable, bool isOrdered)
        {
            IsReliable = isReliable;
            IsOrdered = isOrdered;
        }
    }
}