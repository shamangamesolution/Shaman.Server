using System;
using Shaman.Contract.Bundle;
using Shaman.Contract.Routing.Actualization;

namespace Shaman.Launchers.Common.Balancing;

public class ServerStateHolder : IServerStateProvider, IServerStateUpdater
{
    private string actualState;
    public string Get()
    {
        return actualState;
    }

    public void Update(string state)
    {
        actualState = state ?? throw new ArgumentNullException(nameof(state));
    }
}