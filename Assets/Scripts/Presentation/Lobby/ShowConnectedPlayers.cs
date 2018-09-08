using System;
using System.Collections.Generic;
using Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Networking.Presentation.Lobby
{
    public class ShowConnectedPlayers : MonoBehaviour
    {
        private NetworkManagerWrapper _manager;
        private Dictionary<int, GameObject> _lines;
        private GameObject _textObject;
        private List<Player> _players;

        private void Awake()
        {
            _manager = FindObjectOfType<NetworkManagerWrapper>();
            _manager.Players.OnPlayerConnected += CreateLine;
            _manager.Players.OnPlayerDisconnected += RemoveLine;
            _players = _manager.Players.GetPlayers();

            _lines = new Dictionary<int, GameObject>();
            PopulateList();
        }

        private void PopulateList()
        {
            _textObject = Resources.Load<GameObject>("Networking/Lobby/InfoText");
            for (int i = 0; i < _players.Count; i++)
            {
                CreateLine(_players[i]);
            }
        }

        private void RemoveLine(Player player)
        {
            if (!_lines.ContainsKey(player.Id)) return;
            
            Destroy(_lines[player.Id]);
            _lines.Remove(player.Id);
        }

        private void CreateLine(Player player)
        {
            if (_lines.ContainsKey(player.Id)) return;

            var tmp = Instantiate(_textObject, transform);
            var tx = tmp.GetComponent<Text>();
            tx.text = string.IsNullOrEmpty(player.name) ? "Player" : player.name;

            _lines.Add(player.Id, tmp);
        }
    }
}