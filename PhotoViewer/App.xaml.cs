using System.Windows;
using PhotoViewer.Utils;

namespace PhotoViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private const Boolean Debug = true;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (Debug)
        {
            Logger.OpenLoggerInTerminal();
        }
    }
    

}