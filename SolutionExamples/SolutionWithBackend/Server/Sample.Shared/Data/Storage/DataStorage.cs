using System;
using System.Linq;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Entity.Authentication;
using Sample.Shared.Data.Entity.Currency;
using Sample.Shared.Data.Entity.Progress;
using Sample.Shared.Data.Entity.Shopping;
using Sample.Shared.Extensions;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages;
using Shaman.Messages.Extensions;
using Shaman.Messages.General.Entity;

namespace Sample.Shared.Data.Storage
{
    public class DataStorage : EntityBase
    {

        public Player Player = null;
        private IShamanLogger _logger;
        private void Lock()
        {
            if (IsLocked())
                throw new Exception("Storage is locked - some operation is under processing. Use IsLocked() operator to check storage status");

            IsStorageLocked = true;
        }

        private void Unlock()
        {
            IsStorageLocked = false;
        }

        public bool IsLocked()
        {
            return IsStorageLocked;
        }

        /// <summary>
        /// version to check - if it is changed - load storage from server
        /// </summary>
        public string DatabaseVersion { get; set; }
        public string ServerVersion { get; set; }
        private bool IsStorageLocked { get; set; }
        public bool IsInitialized {  get; private set; }
        public EntityDictionary<GlobalParameter> Parameters { get; set; }
        public EntityDictionary<Currency> Currencies { get; set; }
        public EntityDictionary<AuthProvider> AuthProviders { get; set; }
        public EntityDictionary<PlayerLevel> PlayerLevels { get; set; }
        public EntityDictionary<ShopItem> ShopItems { get; set; }
        
        
        #region serialization
        protected override void SerializeBody(ITypeWriter serializer)
        {
            serializer.Write(DatabaseVersion);
            serializer.Write(ServerVersion);
                
            serializer.WriteEntityDictionary(this.Parameters);
                
            serializer.WriteEntityDictionary(this.Currencies);
            
            serializer.WriteEntityDictionary(this.AuthProviders);
            serializer.WriteEntityDictionary(this.PlayerLevels);
            serializer.WriteEntityDictionary(this.ShopItems);



        }

        protected override void DeserializeBody(ITypeReader serializer)
        {
            DatabaseVersion = serializer.ReadString();
            ServerVersion = serializer.ReadString();

            Parameters = serializer.ReadEntityDictionary<GlobalParameter>();
                
            Currencies = serializer.ReadEntityDictionary<Currency>();
            
            AuthProviders = serializer.ReadEntityDictionary<AuthProvider>();
            PlayerLevels = serializer.ReadEntityDictionary<PlayerLevel>();
            ShopItems = serializer.ReadEntityDictionary<ShopItem>();

            FillRelatedEntities();
        }

        #endregion

        #region init storage section
        //default constructor used on server side
        public DataStorage(IShamanLogger logger)
        {
            ResetStorage();
            _logger = logger;
        }

        public DataStorage()
        {
            
        }

        public void ResetStorage()
        {
            this.IsInitialized = false;
            this.DatabaseVersion = string.Empty;
            this.ServerVersion = string.Empty;
            this.Parameters = new EntityDictionary<GlobalParameter>();
            this.Currencies = new EntityDictionary<Currency>();
            this.AuthProviders = new EntityDictionary<AuthProvider>();
            this.PlayerLevels = new EntityDictionary<PlayerLevel>();
            this.ShopItems = new EntityDictionary<ShopItem>();
        }

        public int CalcRemainingPrice(int secondsRemaining, int instantPrice, int totalSeconds)
        {
            int remainingPrice = (int)Math.Round(((float)secondsRemaining * (float)instantPrice) / (float)totalSeconds, 0);

            return remainingPrice < 1 ? 1 : remainingPrice;
        }

        public bool InitStorage(DataStorage otherStorage)
        {
            _logger?.Info("Start storage initialization...");
            var storage = (DataStorage)otherStorage;
            
            ResetStorage();
            
            this.ServerVersion = storage.ServerVersion;
            this.DatabaseVersion = storage.DatabaseVersion;
            this.Parameters = storage.Parameters;
            this.Currencies = storage.Currencies;
            this.AuthProviders = storage.AuthProviders;
            this.PlayerLevels = storage.PlayerLevels;
            this.ShopItems = storage.ShopItems;
            
            _logger?.Info("Start storage consistency check...");
            
            //checkconsistency
            if (ConsistencyCheck())
            {
                _logger?.Info("Storage is consistent, initialization ended");
                IsInitialized = true;
                return true;
            }
            else
            {
                _logger?.Info("Storage is not consistent. See error log.");
                return false;
            }
        }
        #endregion

        public void FillRelatedEntities()
        {

        }
        
        public bool ConsistencyCheck()
        {
            var result = true;
            
            return result;
        }

        public void SetPlayerStaticData(Player player = null)
        {
            if (player == null)
                player = Player;
        }

        public void SetPlayerDelta(Player player)
        {
            Player = player;
        }
        
        public T GetParameterValue<T>(string parameterName)
        {
            var parameter = Parameters.FirstOrDefault(p => p.Name == parameterName);
            if (parameter == null)
                throw new Exception($"Parameter {parameterName} was not found");

            object val = null;
            
            if (typeof(T) == typeof(int))
                val = parameter.GetIntValue();
            if (typeof(T) == typeof(string))
                val = parameter.GetStringValue();
            if (typeof(T) == typeof(float))
                val = parameter.GetFloatValue();
            if (typeof(T) == typeof(bool))
                val = parameter.GetBoolValue();

            if (val != null)
                return (T)val;
            
            throw new Exception($"Unknown parameter type {typeof(T)}");
        }
        
        #region currencies
        public Currency GetCurrency(CurrencyType type)
        {
            if (Currencies.Where(c => c.Type == type).TryFirstOrDefault(out var currency))
                return currency;
            return null;
        }
        #endregion
    }
}