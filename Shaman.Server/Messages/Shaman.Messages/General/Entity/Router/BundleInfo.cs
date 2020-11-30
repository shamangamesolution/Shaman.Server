using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.Entity.Router
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