using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

/// <summary>
/// Datenzugriff auf Benutzer, erweitert um die Suche nach Benutzername.
/// </summary>
public interface IBenutzerRepository : IRepository<Benutzer>
{
    /// <summary>
    /// Sucht einen Benutzer anhand des Benutzernamens (bereits normalisiert).
    /// </summary>
    /// <returns>Der Benutzer oder null, wenn es ihn nicht gibt.</returns>
    Benutzer? GetByBenutzername(string benutzername);
}
