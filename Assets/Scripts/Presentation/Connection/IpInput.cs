﻿using UiGenerics;
using UnityEngine.Networking;

namespace Networking.Presentation.Connection
{
    public class IpInput : InputFieldView
    {
        private NetworkManager _manager;
        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerWrapper>();
        }

        protected override void EndEdit(string text)
        {
            base.EndEdit(text);

            _manager.networkAddress = text;
        }
    }
}