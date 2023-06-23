using System.Windows;
using System.Windows.Threading;

namespace Bit.VideoParty;

public partial class App
{
    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        e.Handled = true;

        Dispatcher.Invoke(() =>
        {
            Clipboard.SetText(e.Exception.ToString());
            MessageBox.Show("Error! More info copied to your clipboard!", "Bit Video Player");
        });
    }
}