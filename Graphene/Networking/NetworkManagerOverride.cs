using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Graphene.Networking.Presentation.Connection;
using Graphene.Networking.Messaging;
using Graphene.Networking.PlayerConnection;
using Graphene.Utils;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Graphene.Networking
{
    public class NetworkManagerOverride : NetworkManagerWrapper
    {
        public LobbyManager Lobby;

        private List<ILoadingWindow> _loadingWindows;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            _loadingWindows = InterfaceHelper.FindObjects<ILoadingWindow>().ToList();
        }

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

        #region Startup
        public override void OnStartServer()
        {
            base.OnStartServer();

            Lobby = new LobbyManager(_serverMessaging, Players);
        }

        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);
            
            if (!NetworkServer.active)
            {
                StartLoading();
                
                Lobby = new LobbyManager(_clientMessaging, Players);
            }

            var op = SceneManager.LoadSceneAsync("Lobby");
            op.completed += (aop) => { NetworkServer.SpawnObjects(); };
        }

        #endregion
        
        #region Client Side

        public override void OnClientConnect(NetworkConnection conn)
        {
            EndLoading();
            base.OnClientConnect(conn);
        }
        
        #endregion
    }
}