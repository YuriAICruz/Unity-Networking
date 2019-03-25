using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking.PlayerConnection
{
    public class Player
    {
        public string name;
        public int Id;
        public int connectionId;

        public bool Ready;
        public int type;

        public int OnScene;

        public Player()
        {
            Id = Guid.NewGuid().GetHashCode();
            name = "Player";
        }

        public Player(int connectionId)
        {
            Id = Guid.NewGuid().GetHashCode();
            this.connectionId = connectionId;
            name = "Player";
        }
    }

    public class PlayerMessage : MessageBase
    {
        public Player player;

        public PlayerMessage()
        {
        }

        public PlayerMessage(Player player)
        {
            this.player = player;
        }
    }

    public class AllPlayersMessage : MessageBase
    {
        public Player[] players;

        public AllPlayersMessage()
        {
        }

        public AllPlayersMessage(List<Player> players)
        {
            this.players = players.ToArray();
        }
    }

    public abstract class ConnectedPlayers
    {
        public event Action<Player> OnPlayerConnected;
        public event Action<Player> OnPlayerUpdated;
        public event Action<Player> OnPlayerDisconnected;

        protected Player LocalPlayer = new Player();
        protected List<Player> Players = new List<Player>();

        protected NetworkClient Client;

        public ConnectedPlayers(string name)
        {
            LocalPlayer = new Player();
            LocalPlayer.name = name;
            AddToList(LocalPlayer);
        }


        protected void DispatchOnPlayerConnected(Player player)
        {
            if (OnPlayerConnected != null) OnPlayerConnected(player);
        }

        protected void DispatchOnPlayerDisconnected(Player player)
        {
            if (OnPlayerDisconnected != null) OnPlayerDisconnected(player);
        }

        public void SetClient(NetworkClient client)
        {
            Client = client;

            if (Client.connection == null)
            {
                throw new NullReferenceException();
            }

            LocalPlayer.connectionId = Client.connection.connectionId;
        }

        protected void AddToList(Player player)
        {
            if (Players.Contains(player)) return;

            Players.Add(player);
        }

        protected void UpdateOnList(Player player)
        {
            if (!Players.Exists(x => x.Id == player.Id)) return;

            var index = Players.FindIndex(x => x.Id == player.Id);
            if (index < 0) return;
            Players[index] = player;

            if (OnPlayerUpdated != null) OnPlayerUpdated(player);
        }

        protected void RemoveFromList(Player player)
        {
            if (!Players.Contains(player)) return;

            Players.Remove(player);
        }

        public Player GetLocalPlayer()
        {
            return LocalPlayer;
        }

        public void SetUserName(string text)
        {
            LocalPlayer.name = text;
        }

        public List<Player> GetPlayers()
        {
            return Players;
        }

        internal virtual void AddPlayer(NetworkConnection conn)
        {
            throw new ProtocolViolationException();
        }

        internal virtual void RemovePlayer(NetworkConnection conn)
        {
            throw new ProtocolViolationException();
        }

        protected virtual void UpdatePlayer(Player player)
        {
            throw new ProtocolViolationException();
        }

        public void SceneLoaded(int sceneIndex)
        {
            LocalPlayer.OnScene = sceneIndex;

            UpdatePlayer(LocalPlayer);
        }

        public bool CheckReadyPlayers()
        {
            var ready = Players.FindAll(x => x.Ready).Count;
            Debug.Log("Ready Players: " + ready);
            return Players.Count == ready;
        }

        public void IsReady()
        {
            var change = !LocalPlayer.Ready;
            LocalPlayer.Ready = true;

            if (change)
                UpdatePlayer(LocalPlayer);
        }

        public void SetType(int type)
        {
            var change = LocalPlayer.type != type;
            LocalPlayer.type = type;
            
            if (change)
                UpdatePlayer(LocalPlayer);
        }
    }
}