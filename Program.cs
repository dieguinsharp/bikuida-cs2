using Bikuida.Config;
using Bikuida.Menu;
using Bikuida.Services;
using System.Windows;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var dumper = new Dumper();
        var offsets = dumper.GetOffsetsByDump();
        if (offsets == null)
            return;

        var config = new EspConfig();
        var wpfApp = new System.Windows.Application
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose
        };

        var mainWindow = new EspMenuWindow(config, offsets);
        wpfApp.Run(mainWindow);
    }
}
