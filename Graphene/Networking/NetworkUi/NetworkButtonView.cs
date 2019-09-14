using System;
using UnityEngine;
using UnityEngine.UI;

namespace Graphene.UiGenerics.NetworkUi
{
    [RequireComponent(typeof(Button))]
#if !UNITY_2019_1_OR_NEWER
    [RequireComponent(typeof(NetworkIdentity))]
#endif
    public class NetworkButtonView  
#if !UNITY_2019_1_OR_NEWER
       : NetworkBehaviour
#endif
    {
#if !UNITY_2019_1_OR_NEWER
        protected Button Button;

        public NetworkInteractionType Interaction;
#endif
        
        void Awake()
        {
#if !UNITY_2019_1_OR_NEWER
            Button = GetComponent<Button>();
            Button.onClick.AddListener(OnClick);
            
            SendMessage("Setup");
            
            CheckToDisable();
#endif
        }

        private void CheckToDisable()
        {
#if !UNITY_2019_1_OR_NEWER
            switch (Interaction)
            {
                case NetworkInteractionType.OnlyServer:
                    if (!NetworkServer.active) gameObject.SetActive(false);
                    break;
                case NetworkInteractionType.OnlyPlayer:
                    if (!isLocalPlayer) gameObject.SetActive(false);
                    break;
                case NetworkInteractionType.All:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
#endif
        }

        protected virtual void OnClick()
        {
#if !UNITY_2019_1_OR_NEWER
            Invoke("Enable", 0.8f);
#endif
        }

        protected void Disable()
        {
#if !UNITY_2019_1_OR_NEWER
            Button.interactable = false;
#endif
        }

        protected void Enable()
        {
#if !UNITY_2019_1_OR_NEWER
            Button.interactable = true;
#endif
        }
    }
}