namespace Shaman.Launchers.Game.Routing
{
    public interface IMatchMakerInfoProvider
    {
        string MatchMakerUrl { get; set; }
    }
    
    public class MatchMakerInfoProvider :IMatchMakerInfoProvider
    {
        public string MatchMakerUrl { get; set; }

        public MatchMakerInfoProvider(string matchMakerUrl)
        {
            MatchMakerUrl = matchMakerUrl;
        }
    }
}