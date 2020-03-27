using System;
using Sample.Shared.Data.Entity.Currency;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Sample.Shared.Data.Entity
{
   
    public class ShortPlayer : EntityBase
    {
        public string GuestId { get; set; }
        public string NickName { get; set; }
        public byte Level { get; set; }
        public int Experience { get; set; }

        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write(GuestId);
            serializer.Write(NickName);
            serializer.Write(Level);
            serializer.Write(Experience);
        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            GuestId = serializer.ReadString();
            NickName = serializer.ReadString();
            Level = serializer.ReadByte();
            Experience = serializer.ReadInt();
        }
    }
    
    public class Player : EntityBase
    {
        public string GuestId { get; set; } = "";
        public string NickName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime LastOnline { get; set; }
        public bool Blocked { get; set; }
        public byte Level { get; set; }
        public int Experience { get; set; }

        public PlayerWallet Wallet { get; set; } = new PlayerWallet();
        
        public Player()
        {
        }
        
        public static Player GetDefaultPlayer()
        {
            return new Player
            {
                Blocked = false,
                Experience = 0,
                LastOnline = DateTime.UtcNow,
                Level = 1,
                NickName = GenerateNickName(),
                GuestId = "",
                RegistrationDate = DateTime.UtcNow,
            };
        }

        private static string GenerateNickName()
        {
            Guid nickNameSuffix = Guid.NewGuid();

            return $"Player_{nickNameSuffix.ToString().Replace("-", "").Remove(5)}";
        }

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.Write(GuestId);
            typeWriter.Write(NickName);
            typeWriter.Write(RegistrationDate.ToBinary());
            typeWriter.Write(LastOnline.ToBinary());
            typeWriter.Write(Blocked);
            typeWriter.Write(Level);
            typeWriter.Write(Experience);
            typeWriter.WriteEntity(Wallet);
        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            GuestId = typeReader.ReadString();
            NickName = typeReader.ReadString();
            RegistrationDate = DateTime.FromBinary(typeReader.ReadLong());
            LastOnline = DateTime.FromBinary(typeReader.ReadLong());       
            Blocked = typeReader.ReadBool();
            Level = typeReader.ReadByte();
            Experience = typeReader.ReadInt();
            Wallet = typeReader.ReadEntity<PlayerWallet>();
        }
    }
}
