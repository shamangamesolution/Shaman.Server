using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;
using Shaman.Messages.General.Entity.Wallet;

namespace Shaman.Messages.General.Entity.Storage
{

    [Serializable]
    public class DataStorage : EntityBase
    {

        public Player Player = null;
        private ISerializerFactory _serializerFactory;
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

        #region serialization
        protected override void SerializeBody(ISerializer serializer)
        {
            serializer.Write(DatabaseVersion);
            serializer.Write(ServerVersion);
                
            serializer.WriteEntityDictionary(this.Parameters);
                
            serializer.WriteEntityDictionary(this.Currencies);
        }

        protected override void DeserializeBody(ISerializer serializer)
        {
            DatabaseVersion = serializer.ReadString();
            ServerVersion = serializer.ReadString();

            Parameters = serializer.ReadEntityDictionary<GlobalParameter>();
                
            Currencies = serializer.ReadEntityDictionary<Currency>();
        }

        #endregion

        #region init storage section
        //default constructor used on server side
        public DataStorage(ISerializerFactory serializerFactory)
        {
            _serializerFactory = serializerFactory;
            ResetStorage();
        }

        public DataStorage(ISerializerFactory serializerFactory, Action<string, bool> logMethod = null)
        {
            ResetStorage();
            _serializerFactory = serializerFactory;
            LogMethod = logMethod;
        }
        
        private static void Log(string message, bool isError = false)
        {
            LogMethod?.Invoke(message, isError);
        }
        private static Action<string, bool> LogMethod = null;
        private static Action<byte[]> GetStorageMethod = null;

        /// <summary>
        /// This constructor should be called then Storage is initiated from Client Side
        /// </summary>
        /// <param name="requestSender"></param>
        public DataStorage(Player player, Action<string, bool> logMethod = null, Action<byte[]> getStorageMethod = null)
        {
            ResetStorage();

            LogMethod = logMethod;
            GetStorageMethod = getStorageMethod;

            Player = player;
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
        }

        public int CalcRemainingPrice(int secondsRemaining, int instantPrice, int totalSeconds)
        {
            int remainingPrice = (int)Math.Round(((float)secondsRemaining * (float)instantPrice) / (float)totalSeconds, 0);

            return remainingPrice < 1 ? 1 : remainingPrice;
        }

        public void InitStorage(DataStorage otherStorage)
        {
            Log("Start storage initialization...");
            var storage = (DataStorage)otherStorage;
            
            ResetStorage();
            
            this.ServerVersion = storage.ServerVersion;
            this.DatabaseVersion = storage.DatabaseVersion;
            this.Parameters = storage.Parameters;
            this.Currencies = storage.Currencies;
            
            Log("Start storage consistency check...");
            //checkconsistency
            ConsistencyCheck();
            Log("Storage is consistent, initialization ended");

            IsInitialized = true;
        }
        #endregion

        public void ConsistencyCheck()
        {

        }

        public void SetPlayerStaticData(Player player = null)
        {
            if (player == null)
                player = Player;

            if (player.Wallet != null)
            {
                foreach (var item in player.Wallet.Items)
                    item.Currency = Currencies.FirstOrDefault(c => c.Id == item.CurrencyId);
            }
        }

        public void SetPlayer(Player player, SerializationRules rules)
        {
            if (Player == null)
                Player = new Player();
            
            player.CopyTo(this.Player, rules);
            
            if (IsInitialized)
                SetPlayerStaticData();
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
    }
}
