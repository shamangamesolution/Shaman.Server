using System.Collections.Generic;
using Shaman.MM.Contract;

namespace MM
{
    public class MyMmResolver : IMmResolver
    {
        public void Configure(IMatchMakingConfigurator matchMaker)
        {
            matchMaker.AddMatchMakingGroup(
                new Dictionary<byte, object>
                {
                    {1, 2},
                    {2, 3}
                });
        }

        public IRoomPropertiesProvider GetRoomPropertiesProvider()
        {
            return new RoomPropertiesProvider(); 
        }
    }
}