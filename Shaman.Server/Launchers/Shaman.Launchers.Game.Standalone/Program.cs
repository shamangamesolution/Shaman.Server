using System.Collections.Generic;

namespace Shaman.Launchers.Game.Standalone
{
    public static class Program
    {
        internal static void Main(string[] args)
        {
            var result = StandaloneServerLauncher.Launch(new Shaman.Launchers.TestBundle.Game(), args, "TestGame", "SomeRegion", "localhost",
                new List<ushort> {23453}, 7005);
            result.ServerTask.Wait();
        }
    }
}