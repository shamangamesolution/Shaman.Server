using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Sample.Shared.Data.Entity.Authentication
{
    public class AuthProvider : EntityBase
    {
        public string Name { get; set; }
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(Id);
            typeWriter.Write(Name);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            this.Id = typeReader.ReadInt();
            this.Name = typeReader.ReadString();
        }
    }
}