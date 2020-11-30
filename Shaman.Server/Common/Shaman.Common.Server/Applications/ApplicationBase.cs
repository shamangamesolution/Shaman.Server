using System.Collections.Generic;
using System.Linq;
using Shaman.Common.Server.Configuration;
using Shaman.Common.Server.Peers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.Sockets;
using Shaman.Common.Utils.TaskScheduling;

namespace Shaman.Common.Server.Applications
{
    public abstract class ApplicationBase<TL, TP> : IApplication
        where TL: class, IPeerListener<TP>, new()
        where TP: class, IPeer, new()
    {
        
        protected readonly List<IPeerListener<TP>> PeerListeners = new List<IPeerListener<TP>>();
        
        protected readonly IShamanLogger Logger;
        protected readonly ITaskSchedulerFactory TaskSchedulerFactory;
        protected readonly ITaskScheduler TaskScheduler;
        protected readonly IApplicationConfig Config;
        protected IPeerCollection<TP> PeerCollection;
        protected readonly ISerializer Serializer;
        protected readonly ISocketFactory SocketFactory;
        protected readonly IRequestSender RequestSender;

        protected ApplicationBase(IShamanLogger logger, IApplicationConfig config, ISerializer serializer, ISocketFactory socketFactory, ITaskSchedulerFactory taskSchedulerFactory, IRequestSender requestSender)
        {
            Logger = logger;
            Config = config;
            Serializer = serializer;
            SocketFactory = socketFactory;
            TaskSchedulerFactory = taskSchedulerFactory;
            TaskScheduler = taskSchedulerFactory.GetTaskScheduler();
            RequestSender = requestSender;
        }

        private void Listen()
        {
            foreach (var listener in PeerListeners)
            {
                listener.Listen();
                Logger.Info($"Listening started on port: {listener.GetListenPort()}");
            }
        }

        public T GetConfigAs<T>() where T : class, IApplicationConfig
        {
            return Config as T;
        }
        
        public abstract void OnStart();
        public abstract void OnShutDown();

        public List<TL> GetListeners()
        {
            return PeerListeners.Select(l => l as TL).ToList();
        }
        
        public virtual void Start()
        {
            Logger.Info($"Starting");
            //initialize peer collection
            PeerCollection = new PeerCollection<TP>(Logger, Serializer, Config);
            Logger.Debug($"Peer collection initialized as {PeerCollection.GetType()}");
            //initialize packers
//            Serializer.InitializeDefaultSerializers(8, $"Simple{this.GetType()}Buffer");
            Logger.Debug($"Serializer factory initialized as {Serializer.GetType()}");
            //initialize listener
            foreach (var port in Config.GetListenPorts())
            {
                var peerListener = new TL();
                peerListener.Initialize(Logger, PeerCollection, Serializer, Config, TaskSchedulerFactory, port, SocketFactory, RequestSender);
                PeerListeners.Add(peerListener);
                Logger.Info($"PeerListener initialized as {peerListener.GetType()} on port {port}");
            }

                                    
            //call child sub logic
            OnStart();
            
            //start listening and processing messages
            Listen();


        }

        public void ShutDown()
        {   
            Logger.Info($"Shutting down");
            
            //disconnect and remove peers
            foreach (var item in PeerCollection.GetAll())
            {
                item.Value.Disconnect(DisconnectReason.ServerShutDown);                
            }            
            PeerCollection.RemoveAll();

            //stop listening
            foreach(var listener in PeerListeners)
                listener.StopListening();
            
            //stop all tasks
            TaskScheduler.RemoveAll();
            
            //call child sub logic
            OnShutDown();
        }
    }
}