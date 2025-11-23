using System.Windows;

namespace PhotoViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private const Boolean DEBUG = true;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (DEBUG)
        {
            Logger.OpenLoggerInTerminal();
        }
    }
    

}