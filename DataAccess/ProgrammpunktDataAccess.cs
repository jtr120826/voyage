using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public class ProgrammpunktDataAccess
{
    public List<Programmpunkt> GetByReise(int reiseId)
    {
        using var context = new AppDbContext();
        return context.Programmpunkte
            .Where(p => p.ReiseId == reiseId)
            .OrderBy(p => p.Datum)
            .ToList();
    }

    public void Add(Programmpunkt punkt)
    {
        using var context = new AppDbContext();
        context.Programmpunkte.Add(punkt);
        context.SaveChanges();
    }

    public void SetErledigt(int id, bool erledigt)
    {
        using var context = new AppDbContext();
        var punkt = context.Programmpunkte.Find(id);
        if (punkt == null) return;

        punkt.Erledigt = erledigt;
        context.SaveChanges();
    }

    public void Delete(int id)
    {
        using var context = new AppDbContext();
        var punkt = context.Programmpunkte.Find(id);
        if (punkt == null) return;

        context.Programmpunkte.Remove(punkt);
        context.SaveChanges();
    }
}
