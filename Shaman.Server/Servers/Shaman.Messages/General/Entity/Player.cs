using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.General.Entity.Wallet;

namespace Shaman.Messages.General.Entity
{

    [Serializable]
    [Flags]
    public enum SerializationRules : short
    {
        GeneralInfo = 0x0,
        BalanceInfo = 0x1,
        AllInfo = 0x10,
        NoneInfo = 0x20,
    }
    
    [Serializable]
    public class Player : EntityBase
    {
        public int Id { get; set; }
        //50
        public string GuestId { get; set; } = "";
        //50
        public string NickName { get; set; }
        //50
        public DateTime RegistrationDate { get; set; }
        public DateTime LastOnline { get; set; }
        public bool Blocked { get; set; }
        public byte Level { get; set; }
        public int Experience { get; set; }
        public PlayerWallet Wallet { get; set; } = new PlayerWallet();        

        public SerializationRules SerializationRules { get; set; } = SerializationRules.AllInfo;
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

            return $"Pilot_{nickNameSuffix.ToString().Replace("-", "").Remove(5)}";
        }

        public byte[] Serialize(ISerializerFactory serializationFactory, SerializationRules serializationRules)
        {
            this.SerializationRules = serializationRules;
            return Serialize(serializationFactory);
        }
        
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write((short)this.SerializationRules);
            
            serializer.Write(this.Id);
            serializer.WriteString(this.GuestId);

            if ((SerializationRules & SerializationRules.GeneralInfo) == SerializationRules.GeneralInfo
                || (SerializationRules & SerializationRules.BalanceInfo) == SerializationRules.BalanceInfo
                || (SerializationRules & SerializationRules.AllInfo) == SerializationRules.AllInfo)
            {
                serializer.WriteEntity(this.Wallet);
            }

            if ((SerializationRules & SerializationRules.GeneralInfo) == SerializationRules.GeneralInfo
                || (SerializationRules & SerializationRules.AllInfo) == SerializationRules.AllInfo)
            {
                serializer.WriteString(this.NickName);
                serializer.Write(this.Blocked);
                serializer.Write(this.Level);
                serializer.Write(this.Experience);
                serializer.Write(this.RegistrationDate.ToBinary());
            }
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            SerializationRules rules = (SerializationRules)serializer.ReadShort();

            Id = serializer.ReadInt();
            GuestId = serializer.ReadString();
            
            if ((rules & SerializationRules.GeneralInfo) == SerializationRules.GeneralInfo
                || (rules & SerializationRules.BalanceInfo) == SerializationRules.BalanceInfo
                || (rules & SerializationRules.AllInfo) == SerializationRules.AllInfo)
            {
                Wallet = serializer.ReadEntity<PlayerWallet>();
            }

            if ((rules & SerializationRules.GeneralInfo) == SerializationRules.GeneralInfo
                || (rules & SerializationRules.AllInfo) == SerializationRules.AllInfo)
            {
                NickName = serializer.ReadString();
                Blocked = serializer.ReadBool();
                Level = serializer.ReadByte();
                Experience = serializer.ReadInt();
                RegistrationDate = new DateTime(serializer.ReadLong());

            }
            
        }
        
        public void CopyTo(Player player, SerializationRules rules = SerializationRules.AllInfo)
        {
            if (player == null)
            {
                player = this;
                return;
            }

            player.Id = this.Id;
            player.GuestId = this.GuestId;

            if ((rules & SerializationRules.GeneralInfo) == SerializationRules.GeneralInfo
                || (rules & SerializationRules.BalanceInfo) == SerializationRules.BalanceInfo
                || (rules & SerializationRules.AllInfo) == SerializationRules.AllInfo)
            {
                player.Wallet = this.Wallet;
            }

            if ((rules & SerializationRules.GeneralInfo) == SerializationRules.GeneralInfo
                || (rules & SerializationRules.AllInfo) == SerializationRules.AllInfo)
            {
                player.NickName = this.NickName;
                player.Blocked = this.Blocked;
                player.Level = this.Level;
                player.Experience = this.Experience;
                player.RegistrationDate = this.RegistrationDate;
            }
        }
    }
}
