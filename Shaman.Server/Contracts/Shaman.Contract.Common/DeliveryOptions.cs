namespace Shaman.Contract.Common
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