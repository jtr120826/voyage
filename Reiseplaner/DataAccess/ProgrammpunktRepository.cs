using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public class ProgrammpunktRepository : Repository<Programmpunkt>, IProgrammpunktRepository
{
    public List<Programmpunkt> GetByReise(int reiseId)
    {
        using var context = new AppDbContext();
        return context.Programmpunkte
            .Where(p => p.ReiseId == reiseId)
            .OrderBy(p => p.Datum)
            .ToList();
    }
}
