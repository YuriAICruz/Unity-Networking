using Graphene.UiGenerics;

namespace Graphene.Networking.Presentation.Connection
{
    public class LoadingWindow : CanvasGroupView, ILoadingWindow
    {
        void Setup()
        {
            Hide();
        }
    }
}