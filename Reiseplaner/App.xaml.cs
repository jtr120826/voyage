using System.Windows;
using Microsoft.EntityFrameworkCore;
using Reiseplaner.DataAccess;
using Reiseplaner.Services;

namespace Reiseplaner;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using (var context = new AppDbContext())
        {
            // Wendet ausstehende Migrations an und erstellt die Datenbank bei Bedarf.
            context.Database.Migrate();
        }

        // Hier werden die konkreten Repositories einmalig an die Services übergeben (Dependency Injection).
        var reiseService = new ReiseService(new ReiseRepository());
        var programmpunktService = new ProgrammpunktService(new ProgrammpunktRepository());

        new MainWindow(reiseService, programmpunktService).Show();
    }
}
