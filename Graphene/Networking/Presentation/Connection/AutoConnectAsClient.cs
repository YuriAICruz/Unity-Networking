using Graphene.UiGenerics;
using UnityEngine.Networking;

namespace Networking.Presentation.Connection
{
    public class AutoConnectAsClient : ButtonView
    {
        private NetworkManagerWrapper _manager;
        private IpInput _ipInput;

        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerWrapper>();
        }

        protected override void OnClick()
        {
            base.OnClick();

            _manager.SweepForServer((b) => _manager.StartClient());
        }
    }
}