using System;
using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Udp.Sockets;

namespace Shaman.Common.Server.Configuration;

public class ListenPortDefinition
{
    public ushort Port { get; set; }
    public ListenProtocol Protocol { get; set; }
}

public static class ApplicationConfigurationExtensions
{
    private const ListenProtocol DefaultListenProtocol = ListenProtocol.Udp;

    public static List<ListenPortDefinition> GetPortDefinitions(this IApplicationConfig config)
    {
        return config.ListenPorts.Split(",").Select(x =>
        {
            var strings = x.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            try
            {
                return new ListenPortDefinition
                {
                    Port = ushort.Parse(strings.First()),
                    Protocol = strings.Length > 0 ? ParseProtocol(strings[1]) : DefaultListenProtocol
                };
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Failed to parse port definition entry '{x}' ({config.ListenPorts}): {e}");
            }
        }).ToList();
    }

    private static ListenProtocol ParseProtocol(string value)
    {
        switch (value.ToLower())
        {
            case "udp":
                return ListenProtocol.Udp;
            case "ws":
                return ListenProtocol.WebSocket;
        }

        throw new NotSupportedException($"Protocol {value} not supported");
    }
}