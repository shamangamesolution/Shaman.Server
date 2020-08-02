using System.Threading.Tasks;
using Shaman.Common.Utils.Logging;
using Shaman.Contract.Common.Logging;
using Shaman.TestTools.Monkeys;

namespace Shaman.Monkeys
{
    public interface IMonkeyBehaviour
    {
        Task Authenticate(Monkey monkey);
        Task Play(Monkey monkey);

        IMonkeyFactory CreateMonkeyFactory(IShamanLogger logger, Options options);
    }
}