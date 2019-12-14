using System;
using System.Collections.Generic;
using Shaman.Messages;
using Shaman.MM.Contract;
using Shaman.MM.Providers;

namespace Shaman.Tests.Providers
{
    public class FakeRoomPropertiesProvider1 : IRoomPropertiesProvider
    {
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            if (playerMatchMakingProperties.ContainsKey(PropertyCode.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[PropertyCode.PlayerProperties.Level])
                {
                    case 1:
                        return 1000;
                    case 2:
                        return 1000;
                    case 3:
                        return 1000;
                    default:
                        return 1000;
                }
            }
            
            throw new ArgumentException();
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            if (playerMatchMakingProperties.ContainsKey(PropertyCode.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[PropertyCode.PlayerProperties.Level])
                {
                    case 1:
                        return 1;
                    case 2:
                        return 2;
                    case 3:
                        return 3;
                    default:
                        return 3;
                }
            }
            throw new ArgumentException();
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            if (playerMatchMakingProperties.ContainsKey(PropertyCode.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[PropertyCode.PlayerProperties.Level])
                {
                    case 1:
                        return 5000;
                    case 2:
                        return 5000;
                    case 3:
                        return 1000;
                    default:
                        return 5000;
                }
            }
            
            throw new ArgumentException();
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return new Dictionary<byte, object>();
        }
    }
    
    public class FakeRoomPropertiesProvider2 : IRoomPropertiesProvider
    {
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            if (playerMatchMakingProperties.ContainsKey(PropertyCode.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[PropertyCode.PlayerProperties.Level])
                {
                    case 1:
                        return 1000;
                    case 2:
                        return 1000;
                    default:
                        return 1000;
                }
            }
            
            throw new ArgumentException();
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            if (playerMatchMakingProperties.ContainsKey(PropertyCode.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[PropertyCode.PlayerProperties.Level])
                {
                    case 1:
                        return 6;
                    case 2:
                        return 6;
                    default:
                        return 6;
                }
            }
            throw new ArgumentException();
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            if (playerMatchMakingProperties.ContainsKey(PropertyCode.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[PropertyCode.PlayerProperties.Level])
                {
                    case 1:
                        return 5000;
                    case 2:
                        return 500;
                    default:
                        return 500;
                }
            }
            
            throw new ArgumentException();
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return new Dictionary<byte, object>();
        }
    }
    
    public class FakeRoomPropertiesProvider3 : IRoomPropertiesProvider
    {
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 250;
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 12;
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return 5000;
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return new Dictionary<byte, object>();
        }
    }
}