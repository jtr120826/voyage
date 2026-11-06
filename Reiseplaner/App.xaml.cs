using System.Windows;
using Microsoft.EntityFrameworkCore;
using Reiseplaner.DataAccess;
using Reiseplaner.Services;

namespace Reiseplaner;

public partial class App : Application
{
    private AuthService _authService = null!;
    private MainWindow _mainWindow = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Fehlende Berechtigungen zeigen wir als Meldung, statt dass die App abstürzt.
        DispatcherUnhandledException += OnUnbehandelterFehler;

        using (var context = new AppDbContext())
        {
            // Wendet ausstehende Migrations an und erstellt die Datenbank bei Bedarf.
            context.Database.Migrate();
        }

        // Hier werden die konkreten Repositories einmalig an die Services übergeben (Dependency Injection).
        var sitzung = new BenutzerSitzung();
        _authService = new AuthService(new BenutzerRepository(), new PasswortHasher(), sitzung);
        var reiseService = new ReiseService(new ReiseRepository(), sitzung);
        var programmpunktService = new ProgrammpunktService(new ProgrammpunktRepository(), sitzung);

        // Solange der Anmeldedialog offen ist, gibt es noch kein Hauptfenster. Die App darf dann nicht
        // von selbst enden, sonst würde sie nach dem Schliessen des letzten Fensters beendet.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        if (!ZeigeAnmeldung())
        {
            Shutdown();
            return;
        }

        _mainWindow = new MainWindow(reiseService, programmpunktService, _authService, sitzung);
        _mainWindow.AbmeldenAngefordert += OnAbmeldenAngefordert;
        MainWindow = _mainWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        _mainWindow.Show();
    }

    private static void OnUnbehandelterFehler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        if (e.Exception is not UnauthorizedAccessException)
        {
            return;
        }

        MessageBox.Show(e.Exception.Message, "Keine Berechtigung", MessageBoxButton.OK, MessageBoxImage.Warning);
        e.Handled = true;
    }

    private void OnAbmeldenAngefordert(object? sender, EventArgs e)
    {
        _authService.MeldeAb();
        _mainWindow.DatenAusblenden();
        _mainWindow.Hide();

        if (!ZeigeAnmeldung())
        {
            Shutdown();
            return;
        }

        _mainWindow.NachAnmeldungAktualisieren();
        _mainWindow.Show();
    }

    // Zeigt den Anmeldedialog (oder die Ersteinrichtung) und meldet zurück, ob sich jemand angemeldet hat.
    private bool ZeigeAnmeldung()
    {
        return new LoginWindow(_authService).ShowDialog() == true;
    }
}
