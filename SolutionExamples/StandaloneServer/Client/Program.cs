using System;
using System.Collections.Generic;
using System.Threading;
using Shaman.Client.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;
using Shaman.Messages.Authorization;
using Shaman.Messages.General.DTO.Requests;
using Shaman.Messages.RoomFlow;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // var messageDeserializer = new MessageDeserializer();

            var httpSender = new HttpSender(new ConsoleLogger(), new BinarySerializer());


            var playerId = Guid.NewGuid();
            var dictionary = new Dictionary<Guid, Dictionary<byte, object>>
            {
                {
                    playerId, new Dictionary<byte, object>
                    {
                    }
                }
            };
            var createRoomRequest = new CreateRoomRequest(new Dictionary<byte, object>
            {
            }, dictionary);
            var res = httpSender.SendRequest<CreateRoomResponse>("http://localhost:7005",
                createRoomRequest).Result;

            Console.Out.WriteLine("res.RoomId = {0}", res.RoomId);

            Console.Out.WriteLine("res.Success = {0}", res.Success);
            
            var peer = new ClientPeer(new ConsoleLogger(), new TaskSchedulerFactory(new ConsoleLogger()), 300, 33);

            peer.OnPackageAvailable = () =>
            {
                IPacketInfo packet;
                while ((packet = peer.PopNextPacket()) != null)
                {
                    var operationCode = MessageBase.GetOperationCode(packet.Buffer, packet.Offset);
                    Console.Out.WriteLine("operationCode = {0}", operationCode);    
                }
                
            };
            
            peer.Connect("localhost", 23453);
            Thread.Sleep(1000);
            peer.Send(new AuthorizationRequest(playerId));
            peer.Send(new JoinRoomRequest(res.RoomId, new Dictionary<byte, object>()));
            Thread.Sleep(1000);
            peer.Send(new PingRequest());
            peer.Send(new PingRequest());

            Console.ReadKey();
        }
    }
}