using CameraSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public class CameraSync : NetworkBehaviour
    {
        [SyncVar]
        public Vector3 Position;

        private CameraManagement _camera;
        private NetworkManagerOverride _manager;

        private void Awake()
        {
            _manager = FindObjectOfType<NetworkManagerOverride>();
            
            _manager.Lobby.OnGameStart += StartCamera; 
            _camera = FindObjectOfType<CameraManagement>();
        }

        private void StartCamera()
        {
            if (NetworkServer.active)
            {
                RpcStartCamera();
            }
        }

        [ClientRpc]
        private void RpcStartCamera()
        {
            _camera.StartMove();
        }
    }
}