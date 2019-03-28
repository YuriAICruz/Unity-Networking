using Graphene.UiGenerics;

namespace Graphene.Networking.Presentation.Connection
{
    public class JoinMatchButton : ButtonView
    {
        private JoinMatchPage _joinPage;
        private SelectConnectionModePage _connectionPage;

        void Setup()
        {
        }

        private void Start()
        {
            _joinPage = FindObjectOfType<JoinMatchPage>();
            _connectionPage = FindObjectOfType<SelectConnectionModePage>();
        }

        protected override void OnClick()
        {
            base.OnClick();

            _joinPage.Show();
            if (_connectionPage != null)
                _connectionPage.Hide();
        }
    }
}