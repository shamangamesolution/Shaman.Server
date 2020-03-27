using System;
using System.Collections.Generic;

namespace Shaman.MM.Managers
{
    public interface IBotManager
    {
        Dictionary<Guid, Dictionary<byte, object>> GetBots(int count);
    }
}