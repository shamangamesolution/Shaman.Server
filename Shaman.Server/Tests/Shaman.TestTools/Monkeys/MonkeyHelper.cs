using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.General.DTO.Responses;

namespace Shaman.TestTools.Monkeys
{
    public static class MonkeyHelper
    {
        public static async Task<MonkeyGroup> PrepareMonkeys(this IMonkeyFactory monkeyFactory, int count, int delayPerMonkey, Func<Monkey, Task> authFunc)
        {
            var clientsAuthTasks = new Task[count];
            var clients = new Monkey[count];
            
            for (var i = 0; i < clientsAuthTasks.Length; i++)
            {
                clients[i] = monkeyFactory.Create();
                clientsAuthTasks[i] = authFunc(clients[i]);
                await Task.Delay(delayPerMonkey);// distribute init load
            }
            
            return new MonkeyGroup
            {
                Monkeys = clients,
                AuthTasks = clientsAuthTasks
            };
        }

        public static async Task<string> SendMonkeys(this MonkeyGroup monkeys, TimeSpan duration, Func<Monkey, Task> playFunc)
        {
            var report = new StringBuilder();

            var clientsJoinTasks = new List<Task>();
            var clients = new List<Monkey>(monkeys.Monkeys);
            var pings = new List<TimeSpan>[monkeys.Monkeys.Length].Select(l => new List<TimeSpan>()).ToList();
            var pingsFails = 0;
            
            for (int i = 0; i < monkeys.AuthTasks.Length; i++)
            {
                await monkeys.AuthTasks[i];
                clientsJoinTasks.Add(playFunc(clients[i]));
            }

            for (var i = 0; i < clients.Count; i++)
            {
                try
                {
                    await clientsJoinTasks[i];
                }
                catch (Exception e) 
                {
                    Console.Out.WriteLine($"Error join: {e}");
                    clientsJoinTasks.RemoveAt(i);
                    clients.RemoveAt(i);
                    pings.RemoveAt(i);
                    i--;
                }
            }

            if (!clients.Any())
            {
                return "No monkeys joined %(";
            }

            try
            {
                var testRoomEvent = new TestRoomEvent(true, 122, 4.668f, new List<int> {2, 15, 1655435345, 234234});
                var stopwatch = Stopwatch.StartNew();
                while (stopwatch.Elapsed < duration)
                {
                    for (var i = 0; i < clients.Count; i++)
                    {
                        clients[i].Peer.SendEvent(testRoomEvent);
                        try
                        {
                            var ping = await clients[i].Peer.SendRequest<PingResponse>(new PingRequest());
                            pings[i].Add(ping.GetElapsedForNow());
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"PING FAILED: {e}");
                            pingsFails++;
                        }
                    }

                    await Task.Delay(100);
                }

            }
            finally
            {
                foreach (var monk in monkeys.Monkeys)
                    monk.Peer.Disconnect();
                
                report.AppendLine($"{clients.Count} monkey(s) joined. Pings:");
                for (int i = 0; i < pings.Count; i++)
                {
                    var clientPings = pings[i];
                    report.Append($"{i} ({clientPings.Count}): ");
                    if (clientPings.Any())
                    {
                        report.AppendLine(
                            $"max ping - {(int)clientPings.Max(m => m.TotalMilliseconds)}, min ping - {(int)clientPings.Min(m => m.TotalMilliseconds)}, avg ping - {(int)clientPings.Average(p => p.TotalMilliseconds)}");    
                    }
                    else
                    {
                        report.AppendLine("no pings were sent");
                    }
                }

                var pingsCount = pings.Select(p => p.Count).Sum();
                if (pingsCount>0)
                    report.AppendLine($"Total fails: {pingsFails} ({pingsFails * 100 / pingsCount}%)");
            }
            return report.ToString();
        }


    }
}