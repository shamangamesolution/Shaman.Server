using Shaman.Serialization;
using Shaman.Serialization.Messages;

namespace Shaman.Router.Messages
{
    public class BundleInfo : EntityBase
    {
        public string Uri { get; set; } = "";
        public int ServerId { get; set; }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(Uri);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Uri = typeReader.ReadString(); 
        }
    }
}