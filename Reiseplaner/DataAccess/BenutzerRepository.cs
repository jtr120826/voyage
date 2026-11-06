using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

/// <summary>
/// Datenzugriff auf Benutzer. Die Abfrage läuft über EF Core (parametrisiert, keine SQL-Injection).
/// </summary>
public class BenutzerRepository : Repository<Benutzer>, IBenutzerRepository
{
    /// <inheritdoc />
    public Benutzer? GetByBenutzername(string benutzername)
    {
        using var context = new AppDbContext();
        return context.Benutzer.FirstOrDefault(b => b.Benutzername == benutzername);
    }
}
