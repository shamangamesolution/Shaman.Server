using System.Collections.Generic;
using Shaman.MM.Contract;

namespace MM
{
    public class MyMmResolver : IMmResolver
    {
        public void Configure(IMatchMakingConfigurator matchMaker)
        {
            matchMaker.AddMatchMakingGroup(12, 250, true, true, 5000, 90,
                new Dictionary<byte, object>
                {
                    {1, 2},
                    {2, 3}
                },
                new Dictionary<byte, object>
                {
                    {1, 2},
                    {2, 3}
                });
        }
    }
}