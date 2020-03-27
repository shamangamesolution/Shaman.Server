using System.Threading.Tasks;

namespace Shaman.TestTools.Monkeys
{
    public class MonkeyGroup
    {
        public Monkey[] Monkeys { get; set; }
        public Task[] AuthTasks { get; set; }
    }
}