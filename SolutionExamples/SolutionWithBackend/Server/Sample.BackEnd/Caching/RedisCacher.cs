using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sample.BackEnd.Config;
using Sample.Shared.Data.Entity;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.Serialization;
using StackExchange.Redis;

namespace Sample.BackEnd.Caching
{
    public class RedisCacher : ICacher
    {

        private const string CachePrefix = "Sample";
        private ConnectionMultiplexer _redis;
        private IDatabase _db;
        private ILogger  _logger;
        private IOptions<BackendConfiguration> _config;
        private ISerializer _serializerFactory;
        
        private object _sync = new object();
        private object _tokens = new object();
        private object _authTokens = new object();

        public RedisCacher(ILogger<RedisCacher> logger,  
            IOptions<BackendConfiguration> config, ISerializer serializerFactory)
        {
            _serializerFactory = serializerFactory;
            _logger = logger;
            _config = config;
            
            _redis = ConnectionMultiplexer.Connect(_config.Value.RedisConnectionString);
            _db = _redis.GetDatabase();
        }
        
        public async Task Put(Player player)
        {
            lock (_sync)
            {
                //write player
                _db.StringSetAsync($"{CachePrefix}.Players: {player.Id}", CompressHelper.Compress(_serializerFactory.Serialize(player)), TimeSpan.FromDays(1));
            }
        }

        public async Task<Player> Get(int playerId)
        {
            var oldPlayerArray = await _db.StringGetAsync($"{CachePrefix}.Players: {playerId}");
            if (oldPlayerArray.IsNullOrEmpty)
            {
                //_logger.LogCritical($"Cache missed for player {playerId}! Returning null...");
                return null;
            }

            var player = _serializerFactory.DeserializeAs<Player>(CompressHelper.Decompress(oldPlayerArray));//EntityBase.DeserializeAs<Player>(_serializerFactory, CompressHelper.Decompress(oldPlayerArray));
            return player;                                         
        }

        public async Task<Guid> CreateToken(int playerId)
        {
            Guid token = Guid.NewGuid();
            _db.StringSetAsync(token.ToString(), playerId, TimeSpan.FromDays(1));
            return token;
            
        }


        public async Task<int> GetPlayerId(Guid token)
        {
            int playerId = 0;
            if (await _db.KeyExistsAsync(token.ToString()))
                playerId = int.Parse(await _db.StringGetAsync(token.ToString()));
            return playerId;
        }

        public async Task<Guid> GetAuthToken()
        {
            var newToken = Guid.NewGuid();
            lock(_authTokens)
            {
                _db.StringSetAsync(newToken.ToString(), newToken.ToString(), TimeSpan.FromMinutes(1));
                //_logger.LogCritical($"Creating token: {newToken}");
            }
            return newToken;
        }


        public async Task<bool> ValidateAuthToken(Guid token)
        {
            //_logger.LogCritical($"Validating token: {token}");
            
            if (await _db.KeyExistsAsync($"{token}"))
                return true;

            return false;

        }
        
        public async Task RemoveFromCache(int playerId)
        {
            lock (_sync)
            {
                //_players.Remove($"RW.Players: {playerId}");
                _db.KeyDeleteAsync($"{CachePrefix}.Players: {playerId}");
            }
        }
    }
}