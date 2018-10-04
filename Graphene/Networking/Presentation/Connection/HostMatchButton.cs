using Graphene.UiGenerics;

namespace Networking.Presentation.Connection
{
    public class HostMatchButton : ButtonView
    {
        private HostMatchPage _hostPage;
        private SelectConnectionModePage _connectionPage;

        void Setup()
        {
        }

        private void Start()
        {
            _hostPage = FindObjectOfType<HostMatchPage>();
            _connectionPage = FindObjectOfType<SelectConnectionModePage>();
        }

        protected override void OnClick()
        {
            base.OnClick();

            _hostPage.Show();
            if (_connectionPage != null)
                _connectionPage.Hide();
        }
    }
}