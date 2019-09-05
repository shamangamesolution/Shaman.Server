using System;
using System.Threading.Tasks;
using Shaman.BackEnd.Data.Repositories.Interfaces;
using Shaman.Messages.General.Entity;
using Shaman.Messages.General.Entity.Storage;
using Shaman.Messages.General.Entity.Wallet;
using Shaman.Shared.Caching;

namespace Shaman.BackEnd.Data.PlayerStorage
{
    public class PlayerStorage : IPlayerStorage
    {
        private Player _player = null;
        private ICacher _cacher;

        private IPlayerRepository _playerRepo;       
        private ITempRepository _tempRepo;
        private DataStorage _storage;
        
        public PlayerStorage(ICacher cacher, IPlayerRepository playerRepo, ITempRepository tempRepo)
        {
            this._cacher = cacher;
            this._playerRepo = playerRepo;
            this._tempRepo = tempRepo;
        }

        public void SetStorage(DataStorage storage)
        {
            this._playerRepo.SetStorage(storage);
            _storage = storage;
        }

        public async Task<Player> GetPlayer()
        {
            if (_player == null)
                throw new Exception("Player storage was not initialized");

            return _player;
        }
        
        public async Task<Player> GetPlayer(int playerId, bool useCache, SerializationRules serializaionRules = SerializationRules.AllInfo)
        {
            if (useCache)
            {
                _player = await _cacher.Get(playerId);
                if (_player == null)
                {
                    _player = await _playerRepo.GetPlayerInfo(playerId, SerializationRules.AllInfo);
                    if (_player != null)
                        _cacher.Put(_player);
                }
            }
            else
            {
                _player = await _playerRepo.GetPlayerInfo(playerId, serializaionRules);
            }

            if (_player == null)
                throw new Exception($"Can not get player {playerId}");

            return _player;
        }

        public async Task PutPlayerToCache()
        {
            await _cacher.Put(_player);
        }
        
        public async Task GiveLevel(byte newLevel)
        {
            _player.Level = newLevel;
            _playerRepo.UpdateLevel(_player.Id, newLevel);

        }

        public async Task ChangeName(string newName)
        {
            _player.NickName = newName;
            _playerRepo.ChangeName(_player.Id, newName);

        }
        
        public async Task UpdateWalletBalance(int currency, int valueToAdd)
        {
            if (valueToAdd < 0 && !_player.Wallet.IsQuantityAvailable(currency, (uint)Math.Abs(valueToAdd)))
                throw new Exception($"Value {valueToAdd} of currency {currency} is not available");
            
            if (!_player.Wallet.IsCurrencyExists(currency) && valueToAdd > 0)
            {
                var newItem = new PlayerWalletItem()
                {
                    CurrencyId = currency,
                    Quantity = (uint)valueToAdd,
                    PlayerId = _player.Id
                };

                newItem.Id = await _playerRepo.CreateWalletItem(newItem);
                
                _player.Wallet.CreateCurrency(newItem);
            }
            else
            {
                if (valueToAdd > 0)
                    _player.Wallet.AddCurrencyValue(currency, (uint)valueToAdd);
                else
                    _player.Wallet.RemoveCurrencyValue(currency, (uint)Math.Abs(valueToAdd));
                
                //get wallet item
                var walletItem = _player.Wallet.GetWalletItem(currency);

                if (walletItem == null)
                    throw new Exception($"Wallet item for currency {currency} not exists");

                _playerRepo.UpdateWalletItem(walletItem.Id, walletItem.Quantity);
            }
            
            //_player.Wallet.AddCurrencyValue(currency, valueToAdd);

//            _playerRepo.UpdateBalance(_player.Id, _player.Silver + silverToAdd, _player.Gold + goldToAdd, 0, _player.Experience + experienceToAdd);
//            await GiveGold(goldToAdd);
//            await GiveSilver(silverToAdd);
//            await GiveExperience(experienceToAdd);
        }
        
        public async Task AddCurrencyValue(int currencyId, uint valueToAdd)
        {
            await UpdateWalletBalance(currencyId, (int)valueToAdd);
        }

        public async Task RemoveCurrencyValue(int currencyId, uint valueToRemove)
        {
            await UpdateWalletBalance(currencyId, (int)(-valueToRemove));

        }

        public async Task UpdateLastOnlineDate(DateTime dateTime)
        {
            _player.LastOnline = dateTime;
            _playerRepo.UpdateLastOnlineDate(_player.Id);
        }

    }
}