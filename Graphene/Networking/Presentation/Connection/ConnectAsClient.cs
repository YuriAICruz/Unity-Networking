using Graphene.UiGenerics;
using UnityEngine.Networking;

namespace Networking.Presentation.Connection
{
    public class ConnectAsClient : ButtonView
    {
        private NetworkManager _manager;
        private IpInput _ipInput;
        
        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerWrapper>();
            
            Disable();
        }

        private void Start()
        {
            _ipInput = FindObjectOfType<IpInput>();
            _ipInput.InputField.onEndEdit.AddListener(EndEdit);
            
            ValueCheck(_ipInput.InputField.text);
        }

        private void EndEdit(string text)
        {
            ValueCheck(text);
        }

        private void ValueCheck(string text)
        {
            if (!string.IsNullOrEmpty(text))
                Enable();
            else
                Disable();
        }

        protected override void OnClick()
        {
            base.OnClick();
            
            _manager.StartClient();
        }
    }
}