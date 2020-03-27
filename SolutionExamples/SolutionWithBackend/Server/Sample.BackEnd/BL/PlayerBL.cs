using Sample.BackEnd.Caching;
using Sample.BackEnd.Data.PlayerStorage;
using Sample.BackEnd.Data.Repositories.Interfaces;
using Sample.Shared.Data.Storage;
using Shaman.Common.Utils.Logging;

namespace Sample.BackEnd.BL
{
    public class PlayerBL
    {
        //init player repo
        private IPlayerRepository _playerRepo = null;
        
        private DataStorage _storage;
        
        private IShamanLogger _logger;
        private ICacher _cacher;
        private IPlayerStorage _playerStorage;
        
        public PlayerBL(IPlayerRepository playerRepoParam,
            DataStorage storage,
            ICacher cacher,
            IPlayerStorage playerStorage,
            IShamanLogger logger)
        {
            this._logger = logger;
            this._playerRepo = playerRepoParam;
            this._storage = storage;
            this._cacher = cacher;
            this._playerStorage = playerStorage;
        }

    }
}
