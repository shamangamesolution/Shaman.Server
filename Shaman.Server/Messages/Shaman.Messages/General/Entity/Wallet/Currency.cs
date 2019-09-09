using System;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;

namespace Shaman.Messages.General.Entity.Wallet
{
    [Serializable]
    public class Currency : EntityBase
    {
        public bool IsRealCurrency { get; set; }
        
        #region serialization
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write(this.IsRealCurrency);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            IsRealCurrency = serializer.ReadBool();
        }
        #endregion
    }
}