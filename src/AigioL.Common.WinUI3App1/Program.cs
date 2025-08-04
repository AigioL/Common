using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using WinRT;

namespace AigioL.Common.WinUI3App1;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        ComWrappersSupport.InitializeComWrappers();
        Application.Start((p) =>
        {
            var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
#pragma warning disable CA1806 // 不要忽略方法结果
            new App();
#pragma warning restore CA1806 // 不要忽略方法结果
        });
    }
}
