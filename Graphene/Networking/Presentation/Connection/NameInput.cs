using Graphene.UiGenerics;

namespace Networking.Presentation.Connection
{
    public class NameInput : InputFieldView
    {
        private NetworkManagerWrapper _manager;
        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerWrapper>();
        }

        protected override void EndEdit(string text)
        {
            base.EndEdit(text);

            _manager.SetUserName(text);
        }
    }
}