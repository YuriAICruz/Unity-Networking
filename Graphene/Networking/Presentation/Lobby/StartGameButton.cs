using Graphene.UiGenerics.NetworkUi;
using UnityEngine.Networking;

namespace Graphene.Networking.Presentation.Lobby
{
    public class StartGameButton : NetworkButtonView
    {
        private NetworkManagerOverride _manager;

        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerOverride>();

            Interaction = NetworkInteractionType.OnlyServer;
            
            Disable();
            
            _manager.Lobby.OnAllClientsReady += Enable;
            _manager.Lobby.OnAllClientsUnready += Disable;
        }

        protected override void OnClick()
        {
            base.OnClick();
            _manager.Lobby.StartGame();
        }
    }
}