using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Debuging;
using Networking.Messaging;
using Networking.PlayerConnection;
using Networking.Presentation.Connection;
using SceneManagement;
using UnityEngine.Networking;
using Utils;

namespace Networking
{
    public class NetworkManagerOverride : NetworkManager
    {
        public event Action OnClientCStarted;
        public event Action<NetworkConnection> OnConnectedToServer;
        public event Action<NetworkConnection> OnDiconnectedFromServer;

        private ServerMessaging _serverMessaging;
        private ClientMessaging _clientMessaging;

        public LobbyManager Lobby;
        public ConnectedPlayers Players;
        private string _userName;

        private List<ILoadingWindow> _loadingWindows;

        private void Awake()
        {
            SceneManager.AssignManager(this.gameObject);

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

        public override void OnStartHost()
        {
            base.OnStartHost();
            ConsoleDebug.Log("Host Started");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            _serverMessaging = new ServerMessaging(this);
            Players = new ServerConnectedPlayers(_serverMessaging, this, _userName);
            Lobby = new LobbyManager(_serverMessaging, Players);

            ConsoleDebug.Log("Server Started");
        }

        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);
            ConsoleDebug.Log("Client Started");
            if (OnClientCStarted != null) OnClientCStarted();

            _clientMessaging = new ClientMessaging(client, this);
            if (!NetworkServer.active)
            {
                StartLoading();

                Players = new ClientConnectedPlayers(_clientMessaging, _userName);
                Lobby = new LobbyManager(_clientMessaging, Players);
            }

            if (this.client.connection == null)
            {
                StartCoroutine(WaitConnection());
            }
            else
            {
                Players.SetClient(this.client);
            }

            SceneManager.LoadScene("Lobby", () => { NetworkServer.SpawnObjects(); });
        }

        private IEnumerator WaitConnection()
        {
            while (this.client.connection == null)
            {
                yield return new WaitForChangedResult();
            }

            Players.SetClient(this.client);
        }

        #endregion

        #region Server Side

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
            Players.AddPlayer(conn);
            ConsoleDebug.Log("Client Connected to Server");
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            Players.RemovePlayer(conn);
            ConsoleDebug.Log("Client Disconnected to server");
        }

        public void SendMessageToAllClients(NetworkMessages msgCode, MessageBase msg)
        {
            _serverMessaging.SendToAll((short) msgCode, msg);
        }

        public void RegisterToMessageOnServer<T>(NetworkMessages msgCode, Action<MessageBase> callback) where T : MessageBase, new()
        {
            var code = (short) msgCode;
            if (_registeredCallbacks.ContainsKey(code))
            {
                _registeredCallbacks[code].Add(callback);
            }
            else
            {
                _registeredCallbacks.Add(code, new List<Action<MessageBase>>() {callback});
            }
            _serverMessaging.RegisterMessaging(code, CallBack<T>);
        }

        public void UnregisterToMessageOnServer(NetworkMessages msgCode, Action<MessageBase> callback)
        {
            var code = (short) msgCode;
            if (_registeredCallbacks.ContainsKey(code))
            {
                _registeredCallbacks[code].Remove(callback);
            }
            if (!_registeredCallbacks.ContainsKey(code) || _registeredCallbacks[code].Count == 0)
                _serverMessaging.UnregisterMessaging(code);
        }

        #endregion

        private readonly Dictionary<short, List<Action<MessageBase>>> _registeredCallbacks = new Dictionary<short, List<Action<MessageBase>>>();

        private void CallBack<T>(NetworkMessage netmsg) where T : MessageBase, new()
        {
            if (!_registeredCallbacks.ContainsKey(netmsg.msgType)) return;

            var msg = netmsg.ReadMessage<T>();
            foreach (var callback in _registeredCallbacks[netmsg.msgType])
            {
                callback(msg);
            }
        }

        #region Client Side

        public override void OnClientConnect(NetworkConnection conn)
        {
            EndLoading();
            base.OnClientConnect(conn);
            if (OnConnectedToServer != null) OnConnectedToServer(conn);
            ConsoleDebug.Log("Client Connected");
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            if (OnDiconnectedFromServer != null) OnDiconnectedFromServer(conn);
            ConsoleDebug.Log("Client Disconnected");
        }


        public void RegisterToMessageOnClient<T>(NetworkMessages msgCode, Action<MessageBase> callback) where T : MessageBase, new()
        {
            var code = (short) msgCode;
            if (_registeredCallbacks.ContainsKey(code))
            {
                _registeredCallbacks[code].Add(callback);
            }
            else
            {
                _registeredCallbacks.Add(code, new List<Action<MessageBase>>() {callback});
            }
            _clientMessaging.RegisterMessaging(code, CallBack<T>);
        }

        public void UnregisterToMessageOnClient(NetworkMessages msgCode, Action<MessageBase> callback)
        {
            var code = (short) msgCode;
            if (_registeredCallbacks.ContainsKey(code))
            {
                _registeredCallbacks[code].Remove(callback);
            }
            if (!_registeredCallbacks.ContainsKey(code) || _registeredCallbacks[code].Count == 0)
                _clientMessaging.UnregisterMessaging((short) msgCode);
        }

        public void SendMessageToServer(NetworkMessages msgCode, MessageBase msg)
        {
            _clientMessaging.Send((short) msgCode, msg);
        }

        #endregion

        public void SetUserName(string text)
        {
            _userName = text;
        }
    }
}