using System.Collections.Generic;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.Messages.General.DTO.Responses.Router
{
    public class GetMatchmakersResponse : ResponseBase
    {
        public List<MatchMakerConfiguration> Matchmakers { get; set; }

        public GetMatchmakersResponse()
            : base(CustomOperationCode.GetMatchmakers)
        {
            Matchmakers = new List<MatchMakerConfiguration>();
        }


        protected override void SerializeResponseBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteList(Matchmakers);
        }

        protected override void DeserializeResponseBody(ITypeReader typeReader)
        {
            Matchmakers = typeReader.ReadList<MatchMakerConfiguration>();
        }
    }
}
