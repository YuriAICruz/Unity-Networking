using Graphene.UiGenerics;
using UnityEngine.Networking;

namespace Graphene.Networking.Presentation.Connection
{
    public class ConnectAsHost : ButtonView
    {
        private NetworkManagerWrapper _manager;
        
        public bool EnableBroadcast;
        
        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerWrapper>();
        }

        protected override void OnClick()
        {
            base.OnClick();

            if (EnableBroadcast)
                _manager.StartHostBroadcast();
            else
                _manager.StartHost();
        }
    }
}