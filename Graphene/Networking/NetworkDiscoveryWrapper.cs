using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Graphene.Networking
{
    public class NetworkDiscoveryWrapper : NetworkDiscovery
    {
        public event Action<string, string> OnReceivedBroadcastEvent;
        
        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
            base.OnReceivedBroadcast(fromAddress, data);
            
            OnReceivedBroadcastEvent?.Invoke(fromAddress, data);
        }
    }
}