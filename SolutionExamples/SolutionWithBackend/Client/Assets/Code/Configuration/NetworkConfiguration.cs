namespace Code.Configuration
{
    public interface INetworkConfiguration
    {
        string RouterUrl { get; set; }
        string ClientVersion { get; set; }
    }

    public class NetworkConfiguration : INetworkConfiguration
    {
        public string RouterUrl { get; set; }
        public string ClientVersion { get; set; }

        public NetworkConfiguration(string routerUrl, string clientVersion)
        {
            RouterUrl = routerUrl;
            ClientVersion = clientVersion;
        }   
    }
}