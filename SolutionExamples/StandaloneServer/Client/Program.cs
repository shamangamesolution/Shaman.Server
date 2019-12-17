using System;
using Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.General.DTO.Requests;

namespace Client
{
    class HandlerSample : IMessageHandler
    {
        public void OnIncoming(PlayerPeer playerPeer, ISerializer serializer, ushort operationCode, byte[] packetBuffer,
            int offset, int length)
        {
            Console.Out.WriteLine($"Message from {playerPeer.PlayerId} with code {operationCode}");
        }

        public void OnJoined(PlayerPeer peer, ISerializer serializer)
        {
            peer.Send(new PingRequest());
            var customBroadCastEvent = new CustomEvent(new byte[] {2, 34});
            peer.Send(customBroadCastEvent);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var peer1 = CreatePeer();
            var peer2 = CreatePeer();

            var serverClient = new ServerClient();

            var roomId = serverClient.CreateRoom(peer1.PlayerId, peer2.PlayerId).Result;

            peer1.RegisterMessageHandler(new HandlerSample());
            peer2.RegisterMessageHandler(new HandlerSample());

            peer1.JoinRoom(roomId);
            peer2.JoinRoom(roomId);

            Console.ReadKey();
        }

        private static PlayerPeer CreatePeer()
        {
            return new PlayerPeer("localhost", 23453);
        }
    }
}