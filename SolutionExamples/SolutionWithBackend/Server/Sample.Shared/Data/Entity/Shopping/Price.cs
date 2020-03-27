using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.Entity.Shopping
{
    public class PriceInfo : EntityBase
    {
        public int CurrencyId { get; set; }
        public float Price { get; set; }

        public PriceInfo(int currencyId, float price)
        {
            CurrencyId = currencyId;
            Price = price;
        }

        public PriceInfo()
        {
            
        }

        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write(CurrencyId);
            serializer.Write(Price);
        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            CurrencyId = serializer.ReadInt();
            Price = serializer.ReadFloat();
        }
    }
}
