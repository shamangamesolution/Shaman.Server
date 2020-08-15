using System.Collections.Generic;

namespace Shaman.Contract.Bundle
{
    public interface IConfig
    {
        string RouterUrl { get; }
        string HostAddress { get; }
        IEnumerable<ushort> GetListenPorts();
    }
}