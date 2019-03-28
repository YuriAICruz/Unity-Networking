using Graphene.Networking.PlayerConnection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace Graphene.Networking.Messaging
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
            if (_client.handlers.ContainsKey(msgType))
            {
                Debug.LogError($"MsgId: {msgType}, already exists, overriding . . .");
            }
            
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