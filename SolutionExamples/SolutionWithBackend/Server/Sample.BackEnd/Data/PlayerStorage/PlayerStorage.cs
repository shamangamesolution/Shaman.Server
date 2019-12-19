using System;
using System.Threading.Tasks;
using Sample.BackEnd.Caching;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.Shared.Data.Entity;
using Sample.Shared.Data.Entity.Currency;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;

namespace Sample.BackEnd.Data.PlayerStorage
{
    public class PlayerStorage : IPlayerStorage
    {
        private Player _player = null;
        private ICacher _cacher;

        private IPlayerRepository _playerRepo;       
        private ITempRepository _tempRepo;
        private readonly IShamanLogger _logger;
        private IStorageContainer _storageContainer;
        
        public PlayerStorage(ICacher cacher, IPlayerRepository playerRepo, ITempRepository tempRepo,
            IShamanLogger logger, IStorageContainer storageContainer)
        {
            this._cacher = cacher;
            this._playerRepo = playerRepo;
            this._tempRepo = tempRepo;
            _storageContainer = storageContainer;
            _logger = logger;
        }


        public async Task<Player> CreatePlayer(string guestId)
        {
            //get default currencies
            var currency1 = _storageContainer.GetStorage().GetCurrency(CurrencyType.Currency1);
            var currency2 = _storageContainer.GetStorage().GetCurrency(CurrencyType.Currency2);
            var currency3 = _storageContainer.GetStorage().GetCurrency(CurrencyType.Currency3);
            if (currency1 == null)
                throw new Exception($"Can not create player - currency1 is not exists");
            if (currency2 == null)
                throw new Exception($"Can not create player - currency2 is not exists");
            if (currency3 == null)
                throw new Exception($"Can not create player - currency3 is not exists");
            var defaultCurrency1Value = _storageContainer.GetStorage()
                .GetParameterValue<int>(SampleParameterNames.DefaultCurrency1Value);
            var defaultCurrency2Value = _storageContainer.GetStorage()
                .GetParameterValue<int>(SampleParameterNames.DefaultCurrency2Value);
            var defaultCurrency3Value = _storageContainer.GetStorage()
                .GetParameterValue<int>(SampleParameterNames.DefaultCurrency3Value);
            
            _player =  await _playerRepo.CreatePlayer(guestId);

            await AddCurrencyValue(currency1.Id, defaultCurrency1Value);
            await AddCurrencyValue(currency2.Id, defaultCurrency2Value);
            await AddCurrencyValue(currency3.Id, defaultCurrency3Value);
            
            return _player;
        }
        
        public async Task<Player> GetPlayer()
        {
            if (_player == null)
                throw new Exception("Player storage was not initialized");

            return _player;
        }
        
        public async Task<Player> GetPlayer(int playerId, bool useCache)
        {
            if (useCache)
            {
                _player = await _cacher.Get(playerId);
                if (_player == null)
                {
                    _player = await _playerRepo.GetPlayerInfo(playerId);
                    if (_player != null)
                        _cacher.Put(_player);
                }
            }
            else
            {
                _player = await _playerRepo.GetPlayerInfo(playerId);
            }

            if (_player == null)
                throw new Exception($"Can not get player {playerId}");

            _storageContainer.GetStorage().SetPlayerStaticData(_player);
            
            return _player;
        }

        public async Task PutPlayerToCache()
        {
            await _cacher.Put(_player);
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
        }
        
        public async Task AddCurrencyValue(int currencyId, int valueToAdd)
        {
            await UpdateWalletBalance(currencyId, valueToAdd);
        }

        public async Task RemoveCurrencyValue(int currencyId, int valueToRemove)
        {
            await UpdateWalletBalance(currencyId, (-valueToRemove));

        }

        public async Task UpdateLastOnlineDate(DateTime dateTime)
        {
            _player.LastOnline = dateTime;
            _playerRepo.UpdateLastOnlineDate(_player.Id);
        }
    }
}