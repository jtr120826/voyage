using Reiseplaner.Dtos;
using Reiseplaner.Models;
using Reiseplaner.Services;

namespace Reiseplaner.Tests.Hilfsmittel;

/// <summary>
/// Gemeinsame Hilfsmittel für Tests.
/// </summary>
public static class TestDaten
{
    /// <summary>
    /// Erzeugt bei jedem Aufruf ein anderes, zufälliges Passwort (32 Zeichen).
    /// So stehen im Repository keine Passwörter, auch keine Test-Passwörter.
    /// </summary>
    public static string ZufaelligesPasswort() => Guid.NewGuid().ToString("N");

    /// <summary>
    /// Erstellt eine Sitzung, in der ein Benutzer mit der angegebenen Rolle angemeldet ist.
    /// </summary>
    public static BenutzerSitzung SitzungAls(Rolle rolle)
    {
        var sitzung = new BenutzerSitzung();
        sitzung.Anmelden(new BenutzerDto { Id = 1, Benutzername = "testbenutzer", Rolle = rolle });
        return sitzung;
    }
}
