using Reiseplaner.Models;

namespace Reiseplaner.Services;

/// <summary>
/// Legt fest, welche Rolle welches Recht hat (Role-Based Access Control).
/// Diese Tabelle ist die einzige Stelle mit dieser Regel, Services und Oberfläche fragen sie ab.
/// </summary>
public static class Berechtigungen
{
    /// <summary>
    /// Prüft, ob die Rolle das Recht hat.
    /// Leser dürfen ansehen, Sachbearbeiter zusätzlich bearbeiten, nur Administratoren löschen und Benutzer verwalten.
    /// </summary>
    public static bool IstErlaubt(Rolle rolle, Recht recht)
    {
        return recht switch
        {
            Recht.Ansehen => true,
            Recht.Bearbeiten => rolle is Rolle.Sachbearbeiter or Rolle.Administrator,
            Recht.Loeschen => rolle == Rolle.Administrator,
            Recht.BenutzerVerwalten => rolle == Rolle.Administrator,
            _ => false
        };
    }
}
