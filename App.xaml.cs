using System.Windows;

namespace Reiseplaner;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DbInitializer.Initialize(); // DB + Tabellen erstellen falls nötig
    }
}
