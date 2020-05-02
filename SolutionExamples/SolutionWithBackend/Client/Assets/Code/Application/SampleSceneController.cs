using System.Collections.Generic;
using System.Threading.Tasks;
using Code.Common;
using Code.Network;
using Sample.Shared.Data.Entity.Gameplay;
using Shaman.Common.Utils.Logging;
using Shaman.Messages;
using UnityEngine;

namespace Code.Application
{
    public class SampleSceneController : MonoBehaviour
    {
        [Inject] 
        public IUnityClientPeer Network;

        [Inject] 
        public IShamanLogger Logger;
    
        // Start is called before the first frame update
        void Start()
        {
            Network.OnConnected += () => OnConnected();
            Network.Connect();
        }

        private async Task OnConnected()
        {
            Logger.Info("Connected to server");
            var mmParameters = new Dictionary<byte, object>
            {
                {PropertyCode.PlayerProperties.GameMode, (byte)GameMode.TeamPlay},
            };

            var joinParameters = new Dictionary<byte, object> ();
            var joinInfo = (await Network.JoinGame(mmParameters, joinParameters));
            Logger.Info("Joined to room");
        }
    
        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
