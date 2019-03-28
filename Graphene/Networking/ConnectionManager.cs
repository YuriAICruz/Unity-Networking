using System;
using System.Collections;
using System.Linq;
using Graphene.Networking;
using Graphene.Networking.PlayerConnection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.XR;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace VzLab.VrNetworking
{
    public class SceneLoadMessage : MessageBase
    {
        public string sceneName;

        public SceneLoadMessage()
        {
        }

        public SceneLoadMessage(string sceneName)
        {
            this.sceneName = sceneName;
        }
    }

    public class SceneLoadMessageTarget : MessageBase
    {
        public string sceneName;
        public int connId;

        public SceneLoadMessageTarget()
        {
        }

        public SceneLoadMessageTarget(string sceneName, int connId)
        {
            this.sceneName = sceneName;
            this.connId = connId;
        }
    }

    public class VrTransform : MessageBase
    {
        public int connectionId;
        public Vector3 position;
        public Quaternion rotation;

        public VrTransform()
        {
        }

        public VrTransform(Vector3 position, Quaternion rotation, int connectionId)
        {
            this.connectionId = connectionId;
            this.position = position;
            this.rotation = rotation;
        }
    }

    public class ConnectionManager : MonoBehaviour
    {
        public event Action<VrTransform> VrPositionUpdate;

        public bool ForceVr;

        public static bool IsVrDevice;

        private bool _isServer;
        private int _tries = 0;

        public string VrScene;

        public string CompanionScene;

        public string OfflineScene;

        private NetworkManagerWrapper _netMan;

        private static ConnectionManager _instance;
        private bool _disconnected;
        private GameObject _canvas;

        public int ViewConnId = -1;

        public int SelectedClient = -1;

        #region Setup

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this.gameObject);
                return;
            }

            _netMan = FindObjectOfType<NetworkManagerWrapper>();
            _netMan.OnConnectedToServer += OnConnected;
            _netMan.OnDiconnectedFromServer += OnDisconnected;
            _netMan.OnServerStarted += OnServerCreated;

            IsVrDevice = ForceVr;

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            _netMan.SweepForServer(OnServerFound);

            if (IsVrDevice)
            {
                SceneManager.LoadScene(VrScene);
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
        }

        private void OnDestroy()
        {
            if (_netMan == null) return;

            _netMan.OnConnectedToServer -= OnConnected;
        }

        private void OnServerCreated()
        {
            _isServer = true;
        }

        private void SetupVrEnv()
        {
            //SceneManager.LoadScene(VrScene);

            _netMan.Players.SceneLoaded(SceneManager.GetSceneByName(VrScene).buildIndex);

            if (_isServer)
            {
                _netMan.RegisterToMessageOnServer<VrTransform>(NetworkMessages.UpdatePositionOnClient, UpdateCameraServerVr);
                _netMan.RegisterToMessageOnServer<SceneLoadMessage>(NetworkMessages.OpenSceneCustom, OpenSceneServerVr);
                _netMan.RegisterToMessageOnServer<SceneLoadMessageTarget>(NetworkMessages.OpenSceneSingleCustom, OpenSingleSceneServerVr);
            }
            else
            {
                _netMan.RegisterToMessageOnClient<SceneLoadMessage>(NetworkMessages.OpenSceneCustom, OpenScene);
                _netMan.RegisterToMessageOnClient<SceneLoadMessage>(NetworkMessages.UpdatePositionOnClient, DoNothing);
                _netMan.RegisterToMessageOnClient<SceneLoadMessage>(NetworkMessages.OpenSceneSingleCustom, DoNothing);
            }

            UpdatePlayerType(0);
        }

        private void SetupCompanionEnv()
        {
            SceneManager.LoadScene(CompanionScene);

            _netMan.Players.SceneLoaded(SceneManager.GetSceneByName(CompanionScene).buildIndex);

            _netMan.Players.OnPlayerUpdated += OnPlayerUpdated;

            if (_isServer)
            {
                _netMan.RegisterToMessageOnServer<SceneLoadMessage>(NetworkMessages.OpenSceneCustom, OpenSceneServerCompanion);
                _netMan.RegisterToMessageOnServer<VrTransform>(NetworkMessages.UpdatePositionOnClient, UpdateCameraServerCompanion);
                _netMan.RegisterToMessageOnServer<SceneLoadMessageTarget>(NetworkMessages.OpenSceneSingleCustom, OpenSingleSceneServerCompanion);
            }
            else
            {
                _netMan.RegisterToMessageOnClient<VrTransform>(NetworkMessages.UpdatePositionOnClient, UpdateCamera);
                _netMan.RegisterToMessageOnClient<SceneLoadMessage>(NetworkMessages.OpenSceneCustom, DoNothing);
                _netMan.RegisterToMessageOnClient<SceneLoadMessage>(NetworkMessages.OpenSceneSingleCustom, DoNothing);
            }

            UpdatePlayerType(1);
        }

        #endregion

        #region NetworkEvents

        private void OnDisconnected(NetworkConnection client)
        {
            _disconnected = true;

            if(!IsVrDevice)
                SceneManager.LoadScene(OfflineScene);

            _netMan.SweepForServer(OnServerFound);
        }

        private void OnConnected(NetworkConnection client)
        {
            if (IsVrDevice)
            {
                SetupVrEnv();
            }
            else
            {
                SetupCompanionEnv();
            }
        }

        private void OnServerFound(bool sucess)
        {
            if (sucess)
            {
                _netMan.StartClient();
                _tries = 0;
            }
            else if (!IsVrDevice && (_tries > 3 || (!_disconnected && !_isServer)))
            {
                _netMan.StartHostBroadcast();
                _tries = 0;
            }
            else
            {
                _tries++;
                _netMan.SweepForServer(OnServerFound);
            }
        }

        private void OnPlayerUpdated(Player player)
        {
            if (ViewConnId < 0 || ViewConnId != player.connectionId) return;

            ViewVrScene(player.OnScene, player.connectionId);
        }

        #endregion

        #region Messaging

        private void OpenSceneServerVr(MessageBase msg)
        {
            OpenScene(msg);

            var ld = (SceneLoadMessage) msg;

            _netMan.SendMessageToAllClients(NetworkMessages.OpenSceneCustom, new SceneLoadMessage(ld.sceneName));
        }

        private void OpenSceneServerCompanion(MessageBase msg)
        {
            var ld = (SceneLoadMessage) msg;

            _netMan.SendMessageToAllClients(NetworkMessages.OpenSceneCustom, new SceneLoadMessage(ld.sceneName));
        }

        private void UpdateCameraServerCompanion(MessageBase msg)
        {
            UpdateCamera(msg);

            var trt = (VrTransform) msg;

            _netMan.SendMessageToAllClients(NetworkMessages.UpdatePositionOnClient, trt);
        }

        private void UpdateCameraServerVr(MessageBase msg)
        {
            var trt = (VrTransform) msg;

            _netMan.SendMessageToAllClients(NetworkMessages.UpdatePositionOnClient, trt);
        }

        private int FindSceneIndex(string name)
        {
            for (int i = 0, n = SceneManager.sceneCountInBuildSettings; i < n; i++)
            {
                var scene = SceneUtility.GetScenePathByBuildIndex(i);

                if (scene.Contains(name))
                    return i;
            }

            return -1;
        }

        private void OpenScene(MessageBase msg)
        {
            var ld = (SceneLoadMessage) msg;

            foreach (var o in GameObject.FindGameObjectsWithTag("Hotspot"))
            {
                o.gameObject.SetActive(false);
            }
            //TODO: system from other project
            //FindObjectOfType<CircleTimer>()?.SetBrightness(0f);

            var index = FindSceneIndex(ld.sceneName);

            StartCoroutine(LoadScene(index));
        }

        private IEnumerator LoadScene(int index)
        {
            yield return new WaitForSeconds(1);

            SceneManager.LoadScene(index);

            _netMan.Players.SceneLoaded(index);
        }

        private void UpdateCamera(MessageBase msg)
        {
            var trt = (VrTransform) msg;

            VrPositionUpdate?.Invoke(trt);
        }

        private void UpdatePlayerType(int type)
        {
            _netMan.Players.SetType(type);
        }

        public void SendVrTransform(Vector3 pos, Quaternion rot)
        {
            var id = _netMan.Players.GetLocalPlayer().connectionId;

            if (_isServer)
            {
                _netMan.SendMessageToAllClients(NetworkMessages.UpdatePositionOnClient, new VrTransform(pos, rot, id));
            }
            else
            {
                _netMan.SendMessageToServer(NetworkMessages.UpdatePositionOnClient, new VrTransform(pos, rot, id));
            }
        }

        public void OpenSceneOnVr(string sceneName)
        {
            if (_isServer)
            {
                _netMan.SendMessageToAllClients(NetworkMessages.OpenSceneCustom, new SceneLoadMessage(sceneName));
            }
            else
            {
                _netMan.SendMessageToServer(NetworkMessages.OpenSceneCustom, new SceneLoadMessage(sceneName));
            }
        }

        public void ViewVrScene(int Index, int connId)
        {
            ViewConnId = connId;

            SceneManager.LoadScene(Index);

            _netMan.Players.SceneLoaded(Index);

            StartCoroutine(CreateCanvas());
        }

        private IEnumerator CreateCanvas()
        {
            yield return null;

            if (_canvas == null)
            {
                _canvas = Instantiate(Resources.Load<GameObject>("CompanionCanvas"));
                DontDestroyOnLoad(_canvas);
            }
        }

        public void ReturnToMenu()
        {
            if (IsVrDevice) return;

            ViewConnId = -1;

            CheckSelectedClient();

            SceneManager.LoadScene(CompanionScene);
        }

        private void CheckSelectedClient()
        {
            if (SelectedClient >= 0 && !_netMan.Players.GetPlayers().Exists(x => x.connectionId == SelectedClient))
            {
                SelectedClient = -1;
            }
        }

        #endregion


        private void DoNothing(MessageBase msg)
        {
        }
        
        public void OpenSceneOnSingleVr(string sceneName)
        {
            CheckSelectedClient();

            if (SelectedClient < 0) return;
            
            if (_isServer)
            {
                _netMan.ServerMessaging.Send((short) NetworkMessages.OpenSceneCustom, NetworkServer.connections.ToList().Find(x => x.connectionId == SelectedClient), new SceneLoadMessage(sceneName));
            }
            else
            {
                _netMan.SendMessageToServer(NetworkMessages.OpenSceneSingleCustom, new SceneLoadMessageTarget(sceneName, SelectedClient));
            }
        }

        private void OpenSingleSceneServerVr(MessageBase msg)
        {
            var ldtgt = (SceneLoadMessageTarget) msg;

            if (_netMan.Players.GetLocalPlayer().connectionId == ldtgt.connId)
            {
                OpenScene(new SceneLoadMessage(ldtgt.sceneName));
                return;
            }
            
            _netMan.ServerMessaging.Send((short) NetworkMessages.OpenSceneCustom, NetworkServer.connections.ToList().Find(x => x.connectionId == ldtgt.connId), new SceneLoadMessage(ldtgt.sceneName));
        }

        private void OpenSingleSceneServerCompanion(MessageBase msg)
        {
            var ldtgt = (SceneLoadMessageTarget) msg;

            _netMan.ServerMessaging.Send((short) NetworkMessages.OpenSceneCustom, NetworkServer.connections.ToList().Find(x => x.connectionId == ldtgt.connId), new SceneLoadMessage(ldtgt.sceneName));
        }

        public void SelectClient(int playerConnectionId)
        {
            SelectedClient = playerConnectionId;
        }
    }
}