using UnityEngine.Networking;
using UnityEngine;

namespace Graphene.Networking
{
    public class CameraSync : NetworkBehaviour
    {
        private void Awake()
        {
            Debug.LogError("Must be in Camera Assembly");
        }

//        [SyncVar]
//        public Vector3 Position;
//
//        private CameraManagement _camera;
//        private NetworkManagerOverride _manager;
//
//        private void Awake()
//        {
//            _manager = FindObjectOfType<NetworkManagerOverride>();
//            
//            _manager.Lobby.OnGameStart += StartCamera; 
//            _camera = FindObjectOfType<CameraManagement>();
//        }
//
//        private void StartCamera()
//        {
//            if (NetworkServer.active)
//            {
//                RpcStartCamera();
//            }
//        }
//
//        [ClientRpc]
//        private void RpcStartCamera()
//        {
//            _camera.StartMove();
//        }
    }
}