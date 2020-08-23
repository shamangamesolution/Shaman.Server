namespace Shaman.Router.Client
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