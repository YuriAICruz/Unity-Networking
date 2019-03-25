using Graphene.UiGenerics;
using UnityEngine.Networking;

namespace Networking.Presentation.Connection
{
    public class StartAsServer : ButtonView
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
                _manager.StartServerBroadcast();
            else
                _manager.StartServer();
        }
    }
}