namespace Shaman.Contract.MM
{
    public interface IMmResolver
    {
        void Configure(IMatchMakingConfigurator configurator);
        IRoomPropertiesProvider GetRoomPropertiesProvider();
    }
}