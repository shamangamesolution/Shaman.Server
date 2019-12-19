using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Sample.Shared.Data.Entity.ExternalAccounts
{
    public class ExternalAccount : EntityBase
    {
        public int AuthProviderId { get; set; }
        public int PlayerId { get; set; }
        
        public string ExternalId { get; set; }
        public string GuestId { get; set; }
        public ShortPlayer Player { get; set; }
        
        public ExternalAccount()
        {

        }
        
        public ExternalAccount(int authProviderId, int playerId, string externalId, string guestId)
        {
            this.AuthProviderId = authProviderId;
            this.PlayerId = playerId;
            this.ExternalId = externalId;
            this.GuestId = guestId;
        }
        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write(AuthProviderId);
            serializer.Write(PlayerId);
            serializer.Write(ExternalId);
            serializer.Write(GuestId);
            serializer.WriteEntity(Player);
        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            AuthProviderId = serializer.ReadInt();
            PlayerId = serializer.ReadInt();
            ExternalId = serializer.ReadString();
            GuestId = serializer.ReadString();
            Player = serializer.ReadEntity<ShortPlayer>();
        }


    }
}