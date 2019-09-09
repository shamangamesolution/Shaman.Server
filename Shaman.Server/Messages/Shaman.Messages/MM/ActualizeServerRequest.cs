using System;
using System.Collections.Generic;
using System.Net;
using Shaman.Common.Utils.Messages;
using Shaman.Common.Utils.Serialization;
using Shaman.Messages.Extensions;

namespace Shaman.Messages.MM
{
    public class ActualizeServerRequest : RequestBase
    {
        public ServerIdentity Id { get; set; }
        public string CreateRoomUrl { get; set; }
        public Dictionary<ushort, int> PeersCountPerPort { get; set; }
        
        public ActualizeServerRequest(ServerIdentity id, string createRoomUrl, Dictionary<ushort, int> peersCountPerPort) : this()
        {
            this.PeersCountPerPort = peersCountPerPort;
            this.Id = id;
            this.CreateRoomUrl = createRoomUrl;
        }

        public ActualizeServerRequest() : base(CustomOperationCode.ServerActualization, "actualize")
        {

        }

        protected override void SerializeRequestBody(ISerializer serializer)
        {
            serializer.WriteEntity(Id);
            serializer.WriteString(CreateRoomUrl);
            serializer.WriteInt(PeersCountPerPort.Count);
            foreach (var item in PeersCountPerPort)
            {
                serializer.WriteUShort(item.Key);
                serializer.WriteInt(item.Value);
            }
        }

        protected override void DeserializeRequestBody(ISerializer serializer)
        {
            this.Id = serializer.ReadEntity<ServerIdentity>();
            CreateRoomUrl = serializer.ReadString();
            PeersCountPerPort = new Dictionary<ushort, int>();
            var count = serializer.ReadInt();
            for (int i = 0; i < count; i++)
            {
                var key = serializer.ReadUShort();
                var value = serializer.ReadInt();
                PeersCountPerPort.Add(key, value);
            }

        }
    }
}