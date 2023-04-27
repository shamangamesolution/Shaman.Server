using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Bro.WsShamanNetwork;
using FluentAssertions;
using NUnit.Framework;
using Shaman.Common.Utils.Logging;
using TaskScheduler = Shaman.Common.Utils.TaskScheduling.TaskScheduler;

namespace Bro.Tests.Network;

[NonParallelizable]
public class WsTests
{
    [Test]
    public async Task WsMessagingTest()
    {
        var port = 3330;
        var consoleLogger = new ConsoleLogger();
        var taskScheduler = new TaskScheduler(consoleLogger);
        var serverSock = new WebSocketServerTransport(taskScheduler, consoleLogger);
        var clientSock = new WebSocketClientTransport(taskScheduler, consoleLogger);
        var clientRcvTask = new TaskCompletionSource<byte[]>();
        var srvRcvTask = new TaskCompletionSource<byte[]>();
        var payloadFromClient = new byte[] {1, 2, 3};
        var payloadFromServer = new byte[] {4, 5, 6};
        serverSock.AddEventCallbacks((point, packet, dispose) =>
            {
                var received = new byte[packet.Length];
                Array.Copy(packet.Buffer, packet.Offset, received, 0, packet.Length);
                srvRcvTask.SetResult(received);
                serverSock.Send(point, payloadFromServer, 0, payloadFromServer.Length, true, true);
                dispose();
            },
            point =>
            {
                Console.Out.WriteLine($"Incoming client to {point}");
                return true;
            },
            (point, info) => { Console.Out.WriteLine($"Client disconnected from server {point}, {info.Reason}"); });
        serverSock.Listen(port);
        clientSock.OnConnected += point =>
        {
            Console.Out.WriteLine("Client connected to server");
            clientSock.Send(payloadFromClient, 0, payloadFromClient.Length, true, true);
        };
        clientSock.OnDisconnected += (point, info) => { };
        clientSock.OnPacketReceived += (point, packet, arg3) =>
        {
            var received = new byte[packet.Length];
            Array.Copy(packet.Buffer, packet.Offset, received, 0, packet.Length);
            clientRcvTask.SetResult(received);
        };
        clientSock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
        var clientReceived = await clientRcvTask.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var serverReceived = await srvRcvTask.Task.WaitAsync(TimeSpan.FromSeconds(2));
        clientReceived.Should().BeEquivalentTo(payloadFromServer);
        serverReceived.Should().BeEquivalentTo(payloadFromClient);
    }

    [Test]
    public async Task BigMessageTest()
    {
        var port = 3331;
        var consoleLogger = new ConsoleLogger();
        var taskScheduler = new TaskScheduler(consoleLogger);
        var serverSock = new WebSocketServerTransport(taskScheduler, consoleLogger);
        var clientSock = new WebSocketClientTransport(taskScheduler, consoleLogger);
        var clientRcvTask = new TaskCompletionSource<byte[]>();
        var srvRcvTask = new TaskCompletionSource<byte[]>();
        var payloadFromClient = Enumerable.Range(0, 1024 * 200).Select(i => (byte) (i % 256)).ToArray();
        var payloadFromServer = Enumerable.Range(0, 1024 * 200).Select(i => (byte) (i % 256)).ToArray();
        serverSock.AddEventCallbacks((point, packet, dispose) =>
            {
                var received = new byte[packet.Length];
                Array.Copy(packet.Buffer, packet.Offset, received, 0, packet.Length);
                srvRcvTask.SetResult(received);
                serverSock.Send(point, payloadFromServer, 0, payloadFromServer.Length, true, true);
                dispose();
            },
            point =>
            {
                Console.Out.WriteLine($"Incoming client to {point}");
                return true;
            },
            (point, info) => { Console.Out.WriteLine($"Client disconnected from server {point}, {info.Reason}"); });
        serverSock.Listen(port);
        clientSock.OnConnected += point =>
        {
            Console.Out.WriteLine("Client connected to server");
            clientSock.Send(payloadFromClient, 0, payloadFromClient.Length, true, true);
        };
        clientSock.OnDisconnected += (point, info) => { };
        clientSock.OnPacketReceived += (point, packet, arg3) =>
        {
            var received = new byte[packet.Length];
            Array.Copy(packet.Buffer, packet.Offset, received, 0, packet.Length);
            clientRcvTask.SetResult(received);
        };
        clientSock.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));
        var clientReceived = await clientRcvTask.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var serverReceived = await srvRcvTask.Task.WaitAsync(TimeSpan.FromSeconds(2));
        Console.Out.WriteLine($"Client received {clientReceived.Length} bytes");
        Console.Out.WriteLine($"Server received {serverReceived.Length} bytes");
        clientReceived.Length.Should().Be(payloadFromServer.Length);
        serverReceived.Length.Should().Be(payloadFromClient.Length);
        clientReceived.Should().BeEquivalentTo(payloadFromServer);
        serverReceived.Should().BeEquivalentTo(payloadFromClient);
    }
}