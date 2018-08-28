using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Networking.Messaging;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking.PlayerConnection
{
    public class ServerConnectedPlayers : ConnectedPlayers
    {
        private ServerMessaging _serverMessaging;

        public ServerConnectedPlayers(ServerMessaging serverMessaging, MonoBehaviour mono, string name) : base(name)
        {
            _serverMessaging = serverMessaging;
            
            mono.StartCoroutine(ManageTimeout());

            _serverMessaging.RegisterMessaging((short) NetworkMessages.UpdatePlayerOnServer, UpdatePlayerServer);
        }
        
        Queue<Action> _addPlayerQueue = new Queue<Action>();
        private bool _isWaitingPlayer;
        private float _lastPlayerCallTime;

        IEnumerator ManageTimeout()
        {
            while (true)
            {
                if (_isWaitingPlayer && _addPlayerQueue.Count > 0)
                {
                    if (Time.time - _lastPlayerCallTime > 15)
                    {
                        _isWaitingPlayer = false;
                        _addPlayerQueue.Dequeue()();
                    }
                }

                yield return new WaitForChangedResult();
            }
        }

        internal override void AddPlayer(NetworkConnection conn)
        {
            if (!NetworkServer.active) return;
            
            if (conn.hostId == -1)
            {
                DispatchOnPlayerConnected(LocalPlayer);

                _serverMessaging.SendToAll((short) NetworkMessages.AddPlayerOnClient, new PlayerMessage(LocalPlayer));
                return;
            }

            if (_isWaitingPlayer)
            {
                _addPlayerQueue.Enqueue(() => AddPlayer(conn));
                return;
            }

            _lastPlayerCallTime = Time.time;
            
            var callback = new NetworkMessagePackgeCallback((short) NetworkMessages.SendPlayerToServer, (msg) =>
            {
                _serverMessaging.UnregisterMessaging((short) NetworkMessages.SendPlayerToServer);

                var plMsg = msg.ReadMessage<PlayerMessage>();

                AddToList(plMsg.player);

                DispatchOnPlayerConnected(plMsg.player);

                _serverMessaging.SendToAll((short) NetworkMessages.AddPlayerOnClient, plMsg);
                

                _isWaitingPlayer = false;
                if (_addPlayerQueue.Count > 0)
                    _addPlayerQueue.Dequeue()();
            });

            _isWaitingPlayer = true;
            _serverMessaging.Send((short) NetworkMessages.GetPlayerFromClient, conn, new AllPlayersMessage(Players), callback);
        }

        internal override void RemovePlayer(NetworkConnection conn)
        {
            if (!NetworkServer.active) return;

            var player = Players.Find(x => x.connectionId == conn.connectionId);

            if (player != null)
            {
                _serverMessaging.SendToAll((short) NetworkMessages.RemovePlayerOnClient, new PlayerMessage(player));

                DispatchOnPlayerDisconnected(player);
            }
        }

        protected override void UpdatePlayer(Player player)
        {
            UpdateOnList(player);
            _serverMessaging.SendToAll((short) NetworkMessages.UpdatePlayerOnClient, new PlayerMessage(player));
        }

        private void UpdatePlayerServer(NetworkMessage netmsg)
        {
            var plmsg = netmsg.ReadMessage<PlayerMessage>();
            UpdateOnList(plmsg.player);
            _serverMessaging.SendToAll((short) NetworkMessages.UpdatePlayerOnClient, plmsg);
        }
    }
}