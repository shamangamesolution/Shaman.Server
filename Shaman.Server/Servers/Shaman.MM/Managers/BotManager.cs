using System;
using System.Collections.Generic;
using Shaman.Messages;

namespace Shaman.MM.Managers
{
    public class BotManager : IBotManager
    {
        public Dictionary<Guid, Dictionary<byte, object>> GetBots(int count)
        {
            var result = new Dictionary<Guid, Dictionary<byte, object>>();
            return result;
        }
    }
}