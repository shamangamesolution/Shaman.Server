using System;
using Shaman.Contract.Bundle;

namespace Shaman.Launchers.Common.Balancing;

public class ServerStateHolder : IServerStateHolder
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