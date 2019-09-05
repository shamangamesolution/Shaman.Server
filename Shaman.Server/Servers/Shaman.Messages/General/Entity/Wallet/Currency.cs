using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.Entity.Wallet
{
    [Serializable]
    public class Currency : EntityBase
    {
        public int Id { get; set; }
        public bool IsRealCurrency { get; set; }
        
        #region serialization
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write(this.Id);
            serializer.Write(this.IsRealCurrency);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            Id = serializer.ReadInt();                
            IsRealCurrency = serializer.ReadBool();
        }
        #endregion
    }
}