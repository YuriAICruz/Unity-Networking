using UnityEngine.Networking;

namespace Networking
{
    public class NetworkMessagePackgeCallback
    {
        public NetworkMessageDelegate callback;
        public short msgType;

        public NetworkMessagePackgeCallback(short msgType, NetworkMessageDelegate callback)
        {
            this.callback = callback;
            this.msgType = msgType;
        }
    }
}