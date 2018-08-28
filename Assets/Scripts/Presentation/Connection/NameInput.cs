using UiGenerics;

namespace Networking.Presentation.Connection
{
    public class NameInput : InputFieldView
    {
        private NetworkManagerOverride _manager;
        void Setup()
        {
            _manager = FindObjectOfType<NetworkManagerOverride>();
        }

        protected override void EndEdit(string text)
        {
            base.EndEdit(text);

            _manager.SetUserName(text);
        }
    }
}