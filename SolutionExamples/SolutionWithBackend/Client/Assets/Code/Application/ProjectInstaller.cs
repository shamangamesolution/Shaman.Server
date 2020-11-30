using System.Threading.Tasks;
using Code.Common;
using Code.Configuration;
using Code.Network;
using Code.Network.WebRequesters;
using Sample.Shared.Data.Storage;
using Shaman.Client;
using Shaman.Client.Peers;
using Shaman.Client.Providers;
using Shaman.Common.Utils.Logging;
using Shaman.Common.Utils.Senders;
using Shaman.Common.Utils.Serialization;
using Shaman.Common.Utils.TaskScheduling;
using SimpleContainer;
using SimpleContainer.Unity.Installers;
using UnityEngine;
using ClientServerInfoProvider = Shaman.Client.Providers.ClientServerInfoProvider;
using TaskScheduler = Shaman.Common.Utils.TaskScheduling.TaskScheduler;

namespace Code.Application
{
    public class ProjectInstaller : MonoInstaller
    {
        [Header("Network")]
        public string RouterUrl;
        public string EditorClientVersion;
        
        [Header("MonoBehaviour Dependencies")]
        public UnityRequestSender requestSender;
        public UnityClientPeer clientPeer;
        public SampleSceneController SampleSceneController;
        
        public override void Install(Container container)
        {
#if !UNITY_STANDALONE && !UNITY_EDITOR
        EditorClientVersion = Application.version;
#endif
            container.RegisterAttribute<InjectAttribute>();

            var logger = new UnityConsoleLogger(LogLevel.Error | LogLevel.Info);
            
            container.Register<INetworkConfiguration, NetworkConfiguration>(Scope.Singleton, new NetworkConfiguration(RouterUrl, EditorClientVersion));
            container.Register<IWebRequesterConfigProvider, WebRequesterConfigProviderHardcoded>(Scope.Singleton);
            container.Register<IShamanClientPeerConfig, UnityClientPeerConfig>(Scope.Singleton, new UnityClientPeerConfig(20, false, 300, 20));
            container.Register<IShamanLogger, UnityConsoleLogger>(Scope.Singleton, logger);
            container.Register<ITaskSchedulerFactory, TaskSchedulerFactory>(Scope.Singleton);
            container.Register<ITaskScheduler, TaskScheduler>(Scope.Singleton, new TaskSchedulerFactory(logger).GetTaskScheduler());
            container.Register<ISerializer, BinarySerializer>(Scope.Singleton);
            container.Register<IClientServerInfoProvider, ClientServerInfoProvider>(Scope.Singleton);
            container.Register<IWebRequester, WebRequesterSystem>(Scope.Singleton);
            container.Register<IRequestSender, UnityRequestSender>(Scope.Singleton, requestSender);
            container.Register<IStorageContainer, ClientStorageContainer>(Scope.Singleton);
            container.Register<IShamanClientPeer, ShamanClientPeer>(Scope.Singleton);
            container.Register<IShamanClientPeerListener, UnityClientPeer>(Scope.Singleton, clientPeer);
            container.Register<IUnityClientPeer, UnityClientPeer>(Scope.Singleton, clientPeer);
            
            container.Register(Scope.Singleton, SampleSceneController);
        }

        public override Task ResolveAsync(Container container)
        {
            //fake resolve here
            return Task.CompletedTask;
        }

        public override Task AfterResolveAsync(Container container)
        {
            clientPeer.Initialize(30);

            return Task.CompletedTask;
        }
    }
}
