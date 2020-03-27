using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.Entity.Shopping
{
    public enum ConditionType
    {
        None = 0,
        Level = 1,
    }
    
    public class ShopItem : EntityBase
    {
        public string ExternalId { get; set; }
        public GameItemType ItemType { get; set;}
        public int ItemId { get; set; }
        public int ItemQuantity { get; set; }
        public ConditionType ConditionType { get; set; }
        public int ConditionValue { get; set; }
        public int CurrencyId { get; set; }
        public float Price { get; set; }
        public int SortOrder { get; set; }
        public bool IsSpecialOffer { get; set; }
        public bool Enabled { get; set; }
        public float OldPrice { get; set; }
        public string OldExternalId { get; set; }
        public int Discount { get; set; }

        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write(this.ExternalId);
            serializer.Write((int)this.ItemType);
            serializer.Write(this.ItemId);
            serializer.Write(this.ItemQuantity);
            serializer.Write((int)this.ConditionType);
            serializer.Write(this.ConditionValue);
            serializer.Write(this.CurrencyId);
            serializer.Write(this.Price);
            serializer.Write(this.SortOrder);
            serializer.Write(this.IsSpecialOffer);
            serializer.Write(this.Enabled);
            serializer.Write(this.OldPrice);
            serializer.Write(this.OldExternalId);
            serializer.Write(this.Discount);
        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            ExternalId = serializer.ReadString();
            ItemType = (GameItemType)serializer.ReadInt();
            ItemId = serializer.ReadInt();
            ItemQuantity = serializer.ReadInt();
            ConditionType = (ConditionType)serializer.ReadInt();
            ConditionValue = serializer.ReadInt();
            CurrencyId = serializer.ReadInt();
            Price = serializer.ReadFloat();
            SortOrder = serializer.ReadInt();
            IsSpecialOffer = serializer.ReadBool();
            Enabled = serializer.ReadBool();
            OldPrice = serializer.ReadFloat();
            OldExternalId = serializer.ReadString();
            Discount = serializer.ReadInt();
        }
    }
}