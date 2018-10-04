using Graphene.UiGenerics;

namespace Networking.Presentation.Connection
{
    public class LoadingWindow : CanvasGroupView, ILoadingWindow
    {
        void Setup()
        {
            Hide();
        }
    }
}