using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shaman.BackEnd.Config;
using Shaman.Common.Utils.Helpers;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.Entity;
using Shaman.ServerSharedUtilities.Caching;
using StackExchange.Redis;

namespace Shaman.BackEnd.Caching
{
    public class RedisCacher : ICacher
    {

        private const string CachePrefix = "MS";
        private ConnectionMultiplexer _redis;
        private IDatabase _db;
        private ILogger  _logger;
        private IOptions<BackendConfiguration> _config;
        private ISerializerFactory _serializerFactory;
        
        private object _sync = new object();
        private object _tokens = new object();
        private object _authTokens = new object();

        public RedisCacher(ILogger<RedisCacher> logger,  
            IOptions<BackendConfiguration> config, ISerializerFactory serializerFactory)
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
                _db.StringSetAsync($"{CachePrefix}.Players: {player.Id}", CompressHelper.Compress(player.Serialize(_serializerFactory, SerializationRules.AllInfo)), TimeSpan.FromDays(1));
            }
        }

        public async Task<Player> Get(int playerId)
        {
            var oldPlayerArray = await _db.StringGetAsync($"SA.Players: {playerId}");
            if (oldPlayerArray.IsNullOrEmpty)
            {
                return null;
            }

            var player = EntityBase.DeserializeAs<Player>(_serializerFactory, CompressHelper.Decompress(oldPlayerArray));
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
            }
            return newToken;
        }


        public async Task<bool> ValidateAuthToken(Guid token)
        {
            if (await _db.KeyExistsAsync($"{token}"))
                return true;

            return false;

        }
        
        public async Task RemoveFromCache(int playerId)
        {
            lock (_sync)
            {
                _db.KeyDeleteAsync($"{CachePrefix}.Players: {playerId}");
            }
        }
    }
}