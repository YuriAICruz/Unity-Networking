using System;
using System.Collections;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace Graphene.Networking.Messaging
{
    public class NetworkMessagingBase
    {
        private readonly MonoBehaviour _mono;

        public NetworkMessagingBase(MonoBehaviour mono)
        {
            _mono = mono;
        }

        public virtual void RegisterMessaging(short msgType, [NotNull] UnityEngine.Networking.NetworkMessageDelegate callback)
        {
            throw new NotImplementedException();
        }

        public virtual void UnregisterMessaging(short msgType)
        {
            throw new NotImplementedException();
        }

        private IEnumerator WaitToBeReeady(NetworkConnection conn, Action callback)
        {
            var time = Time.time;
            while (!conn.isReady || !conn.isConnected)
            {
                yield return new WaitForChangedResult();
                if (Time.time - time > 10)
                    yield break;
            }

            callback();
        }

        public virtual void Send(short msgCode, NetworkConnection conn, MessageBase msg, NetworkMessagePackgeCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void Send(short msgType, MessageBase msg)
        {
            throw new NotImplementedException();
        }


        protected void Wait(NetworkConnection conn, Action action)
        {
            _mono.StartCoroutine(WaitToBeReeady(conn, action));
        }

        public void RegisterMessaging(NetworkMessagePackgeCallback callback)
        {
            RegisterMessaging(callback.msgType, callback.callback);
        }

        public void UnregisterMessaging(NetworkMessagePackgeCallback callback)
        {
            UnregisterMessaging(callback.msgType);
        }
    }
}