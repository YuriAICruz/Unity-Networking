using System;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.Networking;

namespace Networking
{
    public class SpawnPoint : NetworkBehaviour
    {
        [SerializeField] private int _id;

        // [HideInInspector] 
        [SyncVar] public bool IsTaken;

        private NetworkManagerOverride _manager;

        private void Awake()
        {
            _manager = FindObjectOfType<NetworkManagerOverride>();
        }

        private void OnEnable()
        {
            if (NetworkServer.active)
                _manager.RegisterToMessageOnServer<DirectedBoolMessage>(NetworkMessages.SpawnPointState, UpdateValue);
        }

        private void OnDisable()
        {
            if (NetworkServer.active)
                _manager.UnregisterToMessageOnServer(NetworkMessages.SpawnPointState, UpdateValue);
        }


        private void UpdateValue(MessageBase netmsg)
        {
            var msg = ((DirectedBoolMessage) netmsg);
            if (msg.Id == _id)
                UpdateValue(msg.value);
        }

        void UpdateValue(bool taken)
        {
            IsTaken = taken;
        }

        public void SetTaken(bool taken)
        {
            UpdateValue(taken);
            SetTakenOnServer(taken);
        }

        private void SetTakenOnServer(bool taken)
        {
            if (NetworkServer.active)
            {
                UpdateValue(taken);
                return;
            }

            _manager.SendMessageToServer(NetworkMessages.SpawnPointState, new DirectedBoolMessage(taken, _id));
        }
    }
}