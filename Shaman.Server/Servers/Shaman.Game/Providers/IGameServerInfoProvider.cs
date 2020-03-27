using System.Threading.Tasks;

namespace Shaman.Game.Providers
{
    public interface IGameServerInfoProvider
    {
        void Start();
        void Stop();
        Task ActualizeMe();
        string GetMatchMakerWebUrl(int matchMakerId);
    }
}