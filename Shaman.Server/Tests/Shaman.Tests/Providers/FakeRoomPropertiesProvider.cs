using System;
using System.Collections.Generic;
using Shaman.Contract.MM;
using Shaman.Messages;
using Shaman.TestTools.Events;

namespace Shaman.Tests.Providers
{
    public class FakeRoomPropertiesProvider1 : IRoomPropertiesProvider
    {
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            if (playerMatchMakingProperties.ContainsKey(FakePropertyCodes.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[FakePropertyCodes.PlayerProperties.Level])
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
            if (playerMatchMakingProperties.ContainsKey(FakePropertyCodes.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[FakePropertyCodes.PlayerProperties.Level])
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
            if (playerMatchMakingProperties.ContainsKey(FakePropertyCodes.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[FakePropertyCodes.PlayerProperties.Level])
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
            if (playerMatchMakingProperties.ContainsKey(FakePropertyCodes.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[FakePropertyCodes.PlayerProperties.Level])
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
            if (playerMatchMakingProperties.ContainsKey(FakePropertyCodes.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[FakePropertyCodes.PlayerProperties.Level])
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
            if (playerMatchMakingProperties.ContainsKey(FakePropertyCodes.PlayerProperties.Level))
            {
                switch (playerMatchMakingProperties[FakePropertyCodes.PlayerProperties.Level])
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
        private readonly int _mmTick;
        private readonly int _maximumPlayers;
        private readonly int _mmTime;

        public FakeRoomPropertiesProvider3(int mmTick, int maximumPlayers, int mmTime)
        {
            _mmTick = mmTick;
            _maximumPlayers = maximumPlayers;
            _mmTime = mmTime;
        }
        
        public int GetMatchMakingTick(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _mmTick;
        }

        public int GetMaximumPlayers(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _maximumPlayers;
        }

        public int GetMaximumMatchMakingTime(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return _mmTime;
        }

        public Dictionary<byte, object> GetAdditionalRoomProperties(Dictionary<byte, object> playerMatchMakingProperties)
        {
            return new Dictionary<byte, object>();
        }
    }
}