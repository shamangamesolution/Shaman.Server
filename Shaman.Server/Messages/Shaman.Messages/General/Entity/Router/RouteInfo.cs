using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.Entity.Router
{
    public class RouteInfo : EntityBase
    {
        public string Region { get; set; }
        public string PingAddress { get; set; }
        public string MatchMakerAddress { get; set; }
        public ushort MatchMakerPort { get; set; }
        public string BackendAddress { get; set; }
        public string BackendPort { get; set; }
        
        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            throw new System.NotImplementedException();
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            throw new System.NotImplementedException();
        }
    }
}