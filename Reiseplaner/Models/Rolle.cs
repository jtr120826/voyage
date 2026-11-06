namespace Reiseplaner.Models;

/// <summary>
/// Die Rollen, die ein Benutzer haben kann. Was eine Rolle darf, steht in <see cref="Services.Berechtigungen"/>.
/// </summary>
public enum Rolle
{
    Leser,
    Sachbearbeiter,
    Administrator
}
