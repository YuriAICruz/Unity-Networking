﻿using UiGenerics;
using UnityEngine.Networking;

namespace Networking.Presentation.Connection
{
    public class StartAsServer : ButtonView
    {
        private NetworkManager _manager;
        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerOverride>();
        }

        protected override void OnClick()
        {
            base.OnClick();
            _manager.StartServer();
        }
    }
}