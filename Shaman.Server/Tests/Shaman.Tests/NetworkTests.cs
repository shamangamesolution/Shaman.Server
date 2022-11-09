using System.Threading.Tasks;
using NUnit.Framework;
using Shaman.Client.Peers;
using Shaman.Common.Udp.Senders;
using Shaman.Common.Udp.Sockets;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Contract.Common;
using Shaman.LiteNetLibAdapter;

namespace Shaman.Tests;

public class NetworkTests
{
    [Test]
    [Ignore("Manual test")]
    public async Task BigPackageSendTest()
    {
        var logger = new ConsoleLogger();
        var taskSchedulerFactory = new TaskSchedulerFactory(logger);
        var taskScheduler = taskSchedulerFactory.GetTaskScheduler();
        var packetBatchSender = new PacketBatchSender(taskSchedulerFactory, new ClientPacketSenderConfig(1000, 50),
            logger);
        packetBatchSender.Start(false);

        var server = new LiteNetSock(logger);
        server.AddEventCallbacks((point, packet, disposePackage) =>
            {
                logger.LogInfo(
                    $"[server] received from {point} some {packet.Length} ({packet.Buffer[packet.Offset]} msgs)");
                var offsets = PacketInfo.GetOffsetInfo(packet.Buffer, packet.Offset);
                foreach (var item in offsets)
                    logger.LogInfo($"[server] offset: {item.Offset} length: {item.Length}");

                disposePackage();
            },
            point =>
            {
                logger.LogInfo($"[server] connected " + point);
                return true;
            },
            (point, info) => { logger.LogInfo($"[server] disconnected {point} {info.Reason}"); });
        server.Listen(62222);
        var sender = new ServerSender(new LiteNetClientSocketFactory(), logger,
            (packet, action) => { logger.LogInfo($"[client] received some {packet.Length}"); },
            taskScheduler);
        sender.Connect("127.0.0.1", 62222);
        taskScheduler.ScheduleOnInterval(() => { server.Tick(); }, 0, 20);
        await Task.Delay(1000);
        var length = 303075;
        packetBatchSender.AddPacket(sender, new DeliveryOptions(true, false), new Payload(new byte[length], 0, length));
        await Task.Delay(2000);
    }
}