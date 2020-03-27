using Shaman.MM.Contract;

namespace MM
{
    public class MyMmResolver : IMmResolver
    {
        public void Configure(IMatchMakingConfigurator matchMaker)
        {
        }

        public IRoomPropertiesProvider GetRoomPropertiesProvider()
        {
            return new RoomPropertiesProvider(); 
        }
    }
}