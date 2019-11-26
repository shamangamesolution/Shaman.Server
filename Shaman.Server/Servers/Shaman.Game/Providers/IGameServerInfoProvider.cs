using System.Threading.Tasks;
using Shaman.Messages;
using Shaman.Messages.General.Entity.Router;

namespace Shaman.Game.Providers
{
    public interface IGameServerInfoProvider
    {
        void Start();
        void Stop();
        Task ActualizeMe();
    }
}