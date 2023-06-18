using System.Windows;
using System.Windows.Threading;

namespace Bit.VideoParty;

public partial class App
{
    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        Dispatcher.Invoke(() => MessageBox.Show(e.Exception?.ToString() ?? "Unknown error!"));
    }
}