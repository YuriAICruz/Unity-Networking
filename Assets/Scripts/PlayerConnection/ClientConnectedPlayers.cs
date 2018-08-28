using System.Collections.Generic;
using Networking.Messaging;
using UnityEngine.Networking;

namespace Networking.PlayerConnection
{
    public class ClientConnectedPlayers : ConnectedPlayers
    {
        private ClientMessaging _clientMessaging;

        public ClientConnectedPlayers(ClientMessaging clientMessaging, string name) : base(name)
        {
            _clientMessaging = clientMessaging;

            _clientMessaging.RegisterMessaging((short) NetworkMessages.GetPlayerFromClient, SendPlayerBack);

            _clientMessaging.RegisterMessaging((short) NetworkMessages.AddPlayerOnClient, AddPlayerClient);
            _clientMessaging.RegisterMessaging((short) NetworkMessages.RemovePlayerOnClient, RemovePlayerClient);
            _clientMessaging.RegisterMessaging((short) NetworkMessages.UpdatePlayerOnClient, UpdatePlayerClient);
        }

        private void SendPlayerBack(NetworkMessage netmsg)
        {
            var plrsMsg = netmsg.ReadMessage<AllPlayersMessage>();
            Players = new List<Player>(plrsMsg.players);
            foreach (var player in Players)
            {
                DispatchOnPlayerConnected(player);
            }
            _clientMessaging.Send((short) NetworkMessages.SendPlayerToServer, new PlayerMessage(LocalPlayer));
        }

        private void AddPlayerClient(NetworkMessage netmsg)
        {
            var plMsg = netmsg.ReadMessage<PlayerMessage>();
            AddToList(plMsg.player);
            DispatchOnPlayerConnected(plMsg.player);
        }

        private void RemovePlayerClient(NetworkMessage netmsg)
        {
            var plMsg = netmsg.ReadMessage<PlayerMessage>();
            RemoveFromList(plMsg.player);
            DispatchOnPlayerDisconnected(plMsg.player);
        }

        private void UpdatePlayerClient(NetworkMessage netmsg)
        {
            var plmsg = netmsg.ReadMessage<PlayerMessage>();
            UpdateOnList(plmsg.player);
        }
        
        protected override void UpdatePlayer(Player player)
        {
            _clientMessaging.Send((short) NetworkMessages.UpdatePlayerOnServer, new PlayerMessage(player));
        }
    }
}