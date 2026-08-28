using System.Windows;
using Reiseplaner.DataAccess;

namespace Reiseplaner;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DbInitializer.Initialize(); // DB + Tabellen erstellen falls nötig
    }
}
