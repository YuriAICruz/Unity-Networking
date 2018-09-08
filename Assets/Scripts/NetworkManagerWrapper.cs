using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Networking.Messaging;
using Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class NetworkManagerWrapper : NetworkManager
    {
        public event Action OnClientStarted;
        public event Action<NetworkConnection> OnConnectedToServer;
        public event Action<NetworkConnection> OnDiconnectedFromServer;
        
        protected ServerMessaging _serverMessaging;
        protected ClientMessaging _clientMessaging;
        
        public ConnectedPlayers Players;
        
        private string _userName;

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
        
        #region Startup

        public void SetUserName(string text)
        {
            _userName = text;
        }
        
        public override void OnStartHost()
        {
            base.OnStartHost();
            Debug.Log("Host Started");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();

            _serverMessaging = new ServerMessaging(this);
            Players = new ServerConnectedPlayers(_serverMessaging, this, _userName);

            Debug.Log("Server Started");
        }

        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);
            Debug.Log("Client Started");
            if (OnClientStarted != null) OnClientStarted();

            _clientMessaging = new ClientMessaging(client, this);
            if (!NetworkServer.active)
            {
                Players = new ClientConnectedPlayers(_clientMessaging, _userName);
            }

            if (this.client.connection == null)
            {
                StartCoroutine(WaitConnection());
            }
            else
            {
                Players.SetClient(this.client);
            }
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
            Debug.Log("Client Connected to Server");
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            Players.RemovePlayer(conn);
            Debug.Log("Client Disconnected to server");
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
        
        #region Client Side

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            if (OnConnectedToServer != null) OnConnectedToServer(conn);
            Debug.Log("Client Connected");
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            if (OnDiconnectedFromServer != null) OnDiconnectedFromServer(conn);
            Debug.Log("Client Disconnected");
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
    }
}