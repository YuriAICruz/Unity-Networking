using UiGenerics;
using UnityEngine.Networking;

namespace Networking.Presentation.Connection
{
    public class NetworkConnectionCanvas : CanvasGroupView
    {
        private NetworkManagerWrapper _manager;
        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerWrapper>();
            _manager.OnClientStarted += Hide;
            _manager.OnDiconnectedFromServer += Show;
            
            Show();
            
            DontDestroyOnLoad(gameObject);
        }

        private void Show(NetworkConnection value)
        {
            Show();
        }
    }
}