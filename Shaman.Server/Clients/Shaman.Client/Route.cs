namespace Shaman.Client
{
    public class Route
    {
        public int BackendId { get; set; }
        public string Region { get; set; }
        public string Name { get; set; }
        public string PingAddress { get; set; }
        public string BackendProtocol { get; set; }
        public string BackendAddress { get; set; }
        public ushort BackendPort { get; set; }
        public string MatchMakerAddress { get; set; }
        public ushort MatchMakerPort { get; set; }

        public string GetBackendAddress()
        {
            return $"{BackendProtocol}://{BackendAddress}:{BackendPort}";
        }
        
        public Route(string region, string name, string pingAddress, string backendProtocol, string backendAddress,
            ushort backendPort, int backEndId, string matchMakerAddress, ushort matchMakerPort)
        {
            Region = region;
            Name = name;
            PingAddress = pingAddress;
            BackendAddress = backendAddress;
            BackendPort = backendPort;
            MatchMakerAddress = matchMakerAddress;
            MatchMakerPort = matchMakerPort;
            BackendProtocol = backendProtocol;
            BackendId = backEndId;
        }
    }
}