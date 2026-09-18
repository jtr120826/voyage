using System.Windows;
using Microsoft.EntityFrameworkCore;
using Reiseplaner.DataAccess;

namespace Reiseplaner;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        using var context = new AppDbContext();
        context.Database.Migrate(); // wendet ausstehende Migrations an (erstellt DB falls nötig)
    }
}
