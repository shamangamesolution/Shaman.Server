using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;

namespace Shaman.Messages.General.DTO.Requests.Router
{
    public class GetMatchmakersRequest : RequestBase
    {
        public string ClientVersion { get; set; }
        public GameProject Game { get; set; }
        public GetMatchmakersRequest()
            :base(CustomOperationCode.GetMatchmakers, BackEndEndpoints.GetMatchmakers)
        {
            
        }

        public GetMatchmakersRequest(GameProject game, string clientVersion)
            :this()
        {
            this.ClientVersion = clientVersion;
            this.Game = game;
            
            //to prevent nulls

            if (string.IsNullOrEmpty(this.ClientVersion))
                this.ClientVersion = "";
        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.Write(this.ClientVersion);
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            ClientVersion = serializer.ReadString();
        }
        
    }
}
