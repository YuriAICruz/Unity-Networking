using SceneManagement;
using UiGenerics;
using UnityEngine.Networking;

namespace Networking.Presentation.Connection
{
    public class NetworkConnectionCanvas : CanvasGroupView
    {
        private NetworkManagerOverride _manager;
        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerOverride>();
            _manager.OnClientCStarted += Hide;
            _manager.OnDiconnectedFromServer += Show;
            
            Show();
            
            SceneManager.AssigException(this.transform.parent.gameObject);
        }

        private void Show(NetworkConnection value)
        {
            Show();
        }
    }
}