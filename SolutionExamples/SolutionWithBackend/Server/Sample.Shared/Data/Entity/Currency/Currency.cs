using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.Entity.Currency
{
    public enum CurrencyType : byte
    {
        Currency1 = 1,
        Currency2 = 2,
        Currency3 = 3,
        Real = 4,
        PerkUpgradeToken = 5,
        FreePerkUpgradeToken = 6
    }
    public class Currency : EntityBase
    {
        public CurrencyType Type { get; set; }
        public bool IsRealCurrency { get; set; }
        public GameItemType RelatedObjectType { get; set; }
        public int RelatedObjectId { get; set; }
        
        #region serialization
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write((byte)Type);
            typeWriter.Write(this.IsRealCurrency);
            typeWriter.Write((byte)this.RelatedObjectType);
            typeWriter.Write(this.RelatedObjectId);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Type = (CurrencyType)typeReader.ReadByte();
            IsRealCurrency = typeReader.ReadBool();
            RelatedObjectType = (GameItemType) typeReader.ReadByte();
            RelatedObjectId = typeReader.ReadInt();
        }
        #endregion
    }
}