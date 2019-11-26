using Shaman.Common.Utils.Serialization;

namespace Shaman.Common.Utils.Messages
{
    public class TransitItem : EntityBase
    {
        public string Name { get; set; }
        public string Value { get; set; }
        
        #region serialization

        public TransitItem()
        {
            if (Name == null)
                Name = "";
            if (Value == null)
                Value = "";
        }

        public TransitItem(string name, string value)
        {
            Name = name;
            Value = value;

            if (Name == null)
                Name = "";
            if (Value == null)
                Value = "";
        }
        #endregion

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(this.Name);
            typeWriter.Write(this.Value);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Name = typeReader.ReadString();
            Value = typeReader.ReadString();
        }
    }
}