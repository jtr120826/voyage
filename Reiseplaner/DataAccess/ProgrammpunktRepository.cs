using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

/// <summary>
/// Datenzugriff auf Programmpunkte.
/// </summary>
public class ProgrammpunktRepository : Repository<Programmpunkt>, IProgrammpunktRepository
{
    /// <inheritdoc />
    public List<Programmpunkt> GetByReise(int reiseId)
    {
        using var context = new AppDbContext();
        return context.Programmpunkte
            .Where(p => p.ReiseId == reiseId)
            .OrderBy(p => p.Datum)
            .ToList();
    }
}
