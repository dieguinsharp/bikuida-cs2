using Bikuida.Config;
using Bikuida.Menu;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var config = new EspConfig();
        var wpfApp = new System.Windows.Application();
        var app = new EspMenuWindow(config);
        wpfApp.Dispatcher.Invoke(() => app.Show());
        wpfApp.Run();
    }
}
