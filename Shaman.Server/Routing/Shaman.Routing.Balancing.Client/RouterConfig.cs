namespace Shaman.Routing.Balancing.Client
{
    public class RouterConfig
    {
        public RouterConfig(string routerUrl)
        {
            RouterUrl = routerUrl;
        }

        public string RouterUrl { get; }
    }
}