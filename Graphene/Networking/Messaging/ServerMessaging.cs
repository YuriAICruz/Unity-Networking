using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking.Messaging
{
    public class ServerMessaging : NetworkMessagingBase
    {
        public ServerMessaging(MonoBehaviour mono) : base(mono)
        {
        }

        public override void RegisterMessaging(short msgType, [NotNull] NetworkMessageDelegate callback)
        {
            NetworkServer.RegisterHandler(msgType, callback);
        }

        public override void UnregisterMessaging(short msgType)
        {
            NetworkServer.UnregisterHandler(msgType);
        }

        public void Send(short msgCode, NetworkConnection conn, MessageBase msg, NetworkMessagePackgeCallback callback = null)
        {
            if (!conn.isReady || !conn.isConnected)
            {
                Wait(conn, ()=>Send(msgCode, conn, msg, callback));
                return;
            }
            
            if (callback != null)
            {
                RegisterMessaging(callback);
            }
            NetworkServer.SendToClient(conn.connectionId, msgCode, msg);
        }
        
        public void Send(short msgType, MessageBase msg)
        {
            SendToAll(msgType, msg);
        }

        public bool SendToAll(short msgCode, MessageBase msg)
        {
            return NetworkServer.SendToAll(msgCode, msg);
        }
    }
}