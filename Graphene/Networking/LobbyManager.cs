using System;
using System.Collections.Generic;
using System.Linq;
using Networking.Messaging;
using Networking.PlayerConnection;
using Networking.Presentation.Connection;
using Graphene.Utils;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class LobbyManager
    {
        private readonly ClientMessaging _clientMessaging;
        private readonly ServerMessaging _serverMessaging;
        private ConnectedPlayers _connectedPlayers;
        private List<ILoadingWindow> _loadingWindows;
        private int _currentSceneIndex = -1;
        public event Action OnAllClientsReady, OnAllClientsUnready;
        public event Action OnGameStart;

        #region Setup

        public LobbyManager(ServerMessaging serverMessaging, ConnectedPlayers connectedPlayers)
        {
            _serverMessaging = serverMessaging;
            SetupPlayers(connectedPlayers);
        }

        public LobbyManager(ClientMessaging clientMessaging, ConnectedPlayers connectedPlayers)
        {
            _clientMessaging = clientMessaging;
            _clientMessaging.RegisterMessaging((short) NetworkMessages.OpenScene, OpenScene);
            _clientMessaging.RegisterMessaging((short) NetworkMessages.StartGame, StartGameClient);
            SetupPlayers(connectedPlayers);
        }

        private void SetupPlayers(ConnectedPlayers connectedPlayers)
        {
            _loadingWindows = InterfaceHelper.FindObjects<ILoadingWindow>().ToList();
            _connectedPlayers = connectedPlayers;

            _connectedPlayers.OnPlayerConnected += CheckReady;
            _connectedPlayers.OnPlayerDisconnected += CheckReady;
            _connectedPlayers.OnPlayerUpdated += CheckReady;
        }

        #endregion

        #region Client Side

        private void OpenScene(NetworkMessage netmsg)
        {
            var sceneMsg = netmsg.ReadMessage<SceneLoadMessage>();

            var op = SceneManager.LoadSceneAsync(sceneMsg.sceneIndex);
            
            op.completed += (aop) =>
            {
                _connectedPlayers.SceneLoaded(sceneMsg.sceneIndex);
            };
                
            StartLoading();
        }

        private void StartGameClient(NetworkMessage netmsg)
        {
            DispatchGameStart();
            
            EndLoading();
        }

        #endregion

        #region Loading

        private void StartLoading()
        {
            foreach (var loadingWindow in _loadingWindows)
            {
                loadingWindow.Show();
            }
        }

        private void EndLoading()
        {
            foreach (var loadingWindow in _loadingWindows)
            {
                loadingWindow.Hide();
            }
        }

        #endregion

        #region Server Side

        public void StartGame()
        {
            _currentSceneIndex = 2;
            
            var op = SceneManager.LoadSceneAsync(_currentSceneIndex);
            op.completed += (aop) =>
            {
                _connectedPlayers.OnPlayerUpdated +=  SceneLoadCallback;

                _serverMessaging.SendToAll((short) NetworkMessages.OpenScene, new SceneLoadMessage(_currentSceneIndex));
            
                _connectedPlayers.SceneLoaded(_currentSceneIndex);
            };
            
            StartLoading();
        }

        private void SceneLoadCallback(Player player)
        {
            if (player.OnScene != _currentSceneIndex) return;
            
            var players = _connectedPlayers.GetPlayers();

            if (players.Count > players.FindAll(x => x.OnScene == _currentSceneIndex).Count) return;
            
            _serverMessaging.SendToAll((short) NetworkMessages.StartGame, new NullMessage());

            var cam = UnityEngine.Object.Instantiate(Resources.Load<CameraSync>("Networking/CamSync"));
            NetworkServer.Spawn(cam.gameObject);
            DispatchGameStart();
            NetworkServer.SpawnObjects();
            
            EndLoading();
                    
            _connectedPlayers.OnPlayerUpdated -= SceneLoadCallback;
        }

        private void DispatchGameStart()
        {
            if (OnGameStart != null) OnGameStart();
            
//            NetworkServer.SpawnObjects();
        }

        #endregion

        private void CheckReady(Player player)
        {
            if (!player.Ready)
            {
                if (OnAllClientsUnready != null) OnAllClientsUnready();
                return;
            }
            
            CheckReady();
        }

        public void Ready()
        {
            _connectedPlayers.IsReady();
            
            CheckReady();
        }

        public void CheckReady()
        {
            if (!_connectedPlayers.CheckReadyPlayers()) return;

            if (OnAllClientsReady != null) OnAllClientsReady();
        }


        public class SceneLoadMessage : MessageBase
        {
            public int sceneIndex;

            public SceneLoadMessage()
            {
            }

            public SceneLoadMessage(int sceneIndex)
            {
                this.sceneIndex = sceneIndex;
            }
        }
    }
}