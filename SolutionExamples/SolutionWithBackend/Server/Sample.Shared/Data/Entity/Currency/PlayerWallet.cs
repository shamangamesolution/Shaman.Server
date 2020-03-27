using System;
using System.Linq;
using Sample.Shared.Extensions;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages;
using Shaman.Messages.Extensions;

namespace Sample.Shared.Data.Entity.Currency
{
    public class PlayerWallet : EntityBase
    {
        public EntityDictionary<PlayerWalletItem> Items;

        public PlayerWallet()
        {
            Items = new EntityDictionary<PlayerWalletItem>();
        }
        
        public uint GetAvailableQuantity(int currencyId)
        {
            if (Items.Where(i => i.CurrencyId == currencyId).TryFirstOrDefault(out var item))
            {
                return item.Quantity;
            }
            return 0;
        }
        
        public PlayerWalletItem GetWalletItem(int currencyId)
        {
            if (Items.Where(i => i.CurrencyId == currencyId).TryFirstOrDefault(out var item))
            {
                return item;
            }
            return null;
        }
        
        
        public bool IsCurrencyExists(int currencyId)
        {
            return Items.Where(i => i.CurrencyId == currencyId).TryFirstOrDefault(out var item);
        }
        
        public void CreateCurrency(PlayerWalletItem newItem)
        {
            if (IsCurrencyExists(newItem.CurrencyId))
                throw new Exception($"Currency {newItem.CurrencyId} already exists in player wallet");
            
            Items.Add(newItem);
        }
        
        public void AddCurrencyValue(int currencyId, uint valueToAdd)
        {
            var item = GetWalletItem(currencyId);
            if (item == null)
                throw new Exception($"Currency {currencyId} not exists in player wallet. Ues CreateCurrency to create one");

            if (uint.MaxValue - item.Quantity < valueToAdd)
                item.Quantity = uint.MaxValue;
            else                            
                item.Quantity += valueToAdd;
        }
        
        public bool IsQuantityAvailable(int currencyId, uint valueToCheck)
        {
            var item = GetWalletItem(currencyId);
            if (item == null)
                return false;
            
            if (item.Quantity < valueToCheck)
                return false;

            return true;
        }
        
        
        public void RemoveCurrencyValue(int currencyId, uint valueToRemove)
        {
            var item = GetWalletItem(currencyId);
            if (item == null)
                throw new Exception($"Currency {currencyId} not exists in player wallet. Ues CreateCurrency to create one");
            
            if (item.Quantity < valueToRemove)
                throw new Exception($"Not enough currency {currencyId}. Current value = {item.Quantity}");

            item.Quantity -= valueToRemove;
        }
        
        
        #region serialization

        #endregion

        protected override void SerializeBody(ITypeWriter typeWriter)
        {
            typeWriter.WriteEntityDictionary(this.Items);

        }

        protected override void DeserializeBody(ITypeReader typeReader)
        {
            Items = typeReader.ReadEntityDictionary<PlayerWalletItem>();

        }
    }
    
    
    public class PlayerWalletItem : EntityBase
    {
        public int PlayerId { get; set; }
        public int CurrencyId { get; set; }
        public uint Quantity { get; set; }
        
        public Currency Currency { get; set; }
        
        #region serialization

        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write(this.PlayerId);
            serializer.Write(this.CurrencyId);
            serializer.Write(this.Quantity);
        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            PlayerId = serializer.ReadInt();
            CurrencyId = serializer.ReadInt();
            Quantity = serializer.ReadUint();
        }

        #endregion
    }
}