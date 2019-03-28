using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Graphene.Networking.Messaging;
using Graphene.Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Networking;

namespace Graphene.Networking
{
    public class NetworkManagerWrapper : NetworkManager
    {
        public event Action OnClientStarted;
        public event Action OnServerStarted;
        public event Action<NetworkConnection> OnConnectedToServer;
        public event Action<NetworkConnection> OnDiconnectedFromServer;

        protected ServerMessaging _serverMessaging;
        protected ClientMessaging _clientMessaging;

        public ServerMessaging ServerMessaging
        {
            get { return _serverMessaging; }
        }
        public ClientMessaging ClientMessaging
        {
            get { return _clientMessaging; }
        }

        public ConnectedPlayers Players;

        private string _userName;

        private bool _sweeping;

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

            OnServerStarted?.Invoke();
            
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
            //base.OnClientConnect(conn);

            if (this.clientLoadedScene)
                return;
            ClientScene.Ready(conn);

            if (OnConnectedToServer != null) OnConnectedToServer(conn);
            Debug.Log("Client Connected");
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            if (OnDiconnectedFromServer != null) OnDiconnectedFromServer(conn);
            Debug.Log("Client Disconnected");
            
            base.OnClientDisconnect(conn);
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

        #region Broadcast

        public void StartServerBroadcast()
        {
            StartServer();

            StartCoroutine(StartBroadcasting());
        }

        public void StartHostBroadcast()
        {
            StartHost();

            StartCoroutine(StartBroadcasting());
        }

        IEnumerator StartBroadcasting()
        {
            var ntd = GetComponent<NetworkDiscoveryWrapper>();

            if (ntd == null)
                ntd = gameObject.AddComponent<NetworkDiscoveryWrapper>();

            ntd.Initialize();

            var running = ntd.StartAsServer();
            while (!running)
            {
                running = ntd.StartAsServer();
                yield return null;
            }
        }

        public void SweepForServer(Action<bool> callback)
        {
            StartCoroutine(SweeperCallback(callback));
        }

        IEnumerator SweeperCallback(Action<bool> callback)
        {
            var ntd = GetComponent<NetworkDiscoveryWrapper>();

            if (ntd == null)
                ntd = gameObject.AddComponent<NetworkDiscoveryWrapper>();

            ntd.Initialize();

            var running = ntd.StartAsClient();
            while (!running)
            {
                running = ntd.StartAsClient();
                yield return null;
            }

            ntd.OnReceivedBroadcastEvent += UpdateAddress;
            _sweeping = true;

            var t = 0f;
            while (_sweeping && t < 10) //TODO: 10 seconds timeout hardcoded
            {
                t += Time.deltaTime;
                yield return null;
            }

            Debug.Log("End Sweeping");

            ntd.StopBroadcast();
            ntd.OnReceivedBroadcastEvent -= UpdateAddress;

            callback?.Invoke(!_sweeping);

            _sweeping = false;
        }

        private void UpdateAddress(string fromAddress, string data)
        {
            networkAddress = fromAddress;
            _sweeping = false;
        }

        [Obsolete]
        private string[] GetMyIps()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var s = new List<string>();

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    Debug.Log(ip);
                    s.Add(ip.ToString());
                }
            }

            return s.ToArray();
        }

        [Obsolete]
        private void FindServer()
        {
            var myip = GetMyIps();
            Debug.Log($"NetworkInterfaces count: {NetworkInterface.GetAllNetworkInterfaces().Length}");
            var s = myip[0].Split('.');
            Debug.Log($"Subnet: {s[0]}.{s[1]}.{s[2]}");

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (!ip.IsDnsEligible)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            Debug.Log($"ip: {ip.Address}");
                            // All IP Address in the LAN
                        }
                    }
                }
            }

            _sweeping = false;
        }

        #endregion
    }
}