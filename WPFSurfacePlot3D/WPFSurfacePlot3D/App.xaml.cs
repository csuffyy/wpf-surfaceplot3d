using System.Windows;
using System.Windows.Threading;

namespace WPFSurfacePlot3D
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // This app opens MainWindow on start.
        // Thus, you can find the main application logic in the file: MainWindow.xaml.cs

        void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var comException = e.Exception as System.Runtime.InteropServices.COMException;
            if (comException != null && comException.ErrorCode == -2147221040)
            {
                e.Handled = true;
            }
        }
    }
}
