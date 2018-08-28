using UiGenerics;

namespace Networking.Presentation.Connection
{
    public class BachToConnectionSelectionButton : ButtonView
    {
        private HostMatchPage _hostPage;
        private JoinMatchPage _joinPage;
        private SelectConnectionModePage _connectionPage;
        
        void Setup()
        {
        }

        private void Start()
        {
            _hostPage = FindObjectOfType<HostMatchPage>();
            _joinPage = FindObjectOfType<JoinMatchPage>();
            _connectionPage = FindObjectOfType<SelectConnectionModePage>();
        }

        protected override void OnClick()
        {
            base.OnClick();

            _hostPage.Hide();
            _joinPage.Hide();
            _connectionPage.Show();
        }
    }
}