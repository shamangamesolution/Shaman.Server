using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Shaman.Messages.General.Entity.Wallet
{
    [Serializable]
    public class PlayerWallet : EntityBase
    {
        public List<PlayerWalletItem> Items { get; set; }
        public uint GetAvailableQuantity(int currencyId)
        {
            var item = Items.FirstOrDefault(i => i.CurrencyId == currencyId);
            if (item == null)
                return 0;
            return item.Quantity;
        }
        
        public PlayerWalletItem GetWalletItem(int currencyId)
        {
            var item = Items.FirstOrDefault(i => i.CurrencyId == currencyId);
            return item;
        }
        
        
        public bool IsCurrencyExists(int currencyId)
        {
            var item = Items.FirstOrDefault(i => i.CurrencyId == currencyId);
            if (item == null)
                return false;
            return true;
        }
        
        public void CreateCurrency(PlayerWalletItem newItem)
        {
            if (IsCurrencyExists(newItem.CurrencyId))
                throw new Exception($"Currency {newItem.CurrencyId} already exists in player wallet");
            
            Items.Add(newItem);
        }
        
        public void AddCurrencyValue(int currencyId, uint valueToAdd)
        {
            var item = Items.FirstOrDefault(i => i.CurrencyId == currencyId);
            if (item == null)
                throw new Exception($"Currency {currencyId} not exists in player wallet. Ues CreateCurrency to create one");

            if (uint.MaxValue - item.Quantity < valueToAdd)
                item.Quantity = uint.MaxValue;
            else                            
                item.Quantity += valueToAdd;
        }
        
        public bool IsQuantityAvailable(int currencyId, uint valueToCheck)
        {
            var item = Items.FirstOrDefault(i => i.CurrencyId == currencyId);
            if (item == null)
                return false;
            
            if (item.Quantity < valueToCheck)
                return false;

            return true;
        }
        
        
        public void RemoveCurrencyValue(int currencyId, uint valueToRemove)
        {
            var item = Items.FirstOrDefault(i => i.CurrencyId == currencyId);
            if (item == null)
                throw new Exception($"Currency {currencyId} not exists in player wallet. Ues CreateCurrency to create one");
            
            if (item.Quantity < valueToRemove)
                throw new Exception($"Not enough currency {currencyId}. Current value = {item.Quantity}");

            item.Quantity -= valueToRemove;
        }
        
        
        #region serialization
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.WriteList(this.Items);

        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            Items = serializer.ReadList<PlayerWalletItem>();

        }
        #endregion
    }
    
    
    [Serializable]
    public class PlayerWalletItem : EntityBase
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int CurrencyId { get; set; }
        public uint Quantity { get; set; }
        
        public Currency Currency { get; set; }
        
        #region serialization
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write(this.Id);
            serializer.Write(this.PlayerId);
            serializer.Write(this.CurrencyId);
            serializer.Write(this.Quantity);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            Id = serializer.ReadInt();
            PlayerId = serializer.ReadInt();
            CurrencyId = serializer.ReadInt();
            Quantity = serializer.ReadUint();
        }

        #endregion
    }
}