using JetBrains.Annotations;
using Networking.PlayerConnection;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking.Messaging
{
    public class ClientMessaging : NetworkMessagingBase
    {
        private readonly NetworkClient _client;

        public ClientMessaging(NetworkClient client, MonoBehaviour mono) : base(mono)
        {
            _client = client;
        }

        public override void RegisterMessaging(short msgType, [NotNull]NetworkMessageDelegate callback)
        {
            _client.RegisterHandler(msgType, callback);
        }

        public override void UnregisterMessaging(short msgType)
        {
            _client.UnregisterHandler(msgType);
        }

        public void Send(short msgType, MessageBase msg)
        {
            _client.Send(msgType, msg);
        }

        private void SendPlayerBack(NetworkMessage netmsg)
        {
            Send((short)NetworkMessages.SendPlayerToServer, new PlayerMessage());
        }
    }
}