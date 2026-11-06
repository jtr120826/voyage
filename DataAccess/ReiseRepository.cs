using Microsoft.EntityFrameworkCore;
using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public class ReiseRepository : Repository<Reise>
{
    public override List<Reise> GetAll()
    {
        using var context = new AppDbContext();

        // Include() lädt die Programmpunkte pro Reise gleich mit (eine Abfrage statt N+1).
        var reisen = context.Reisen
            .Include(r => r.Programmpunkte)
            .OrderBy(r => r.Startdatum)
            .ToList();

        foreach (var reise in reisen)
        {
            reise.GeplantesBudget = reise.Programmpunkte.Sum(p => p.Kosten);
        }

        return reisen;
    }

    public override void Delete(int id)
    {
        using var context = new AppDbContext();

        var reise = context.Reisen
            .Include(r => r.Programmpunkte)
            .FirstOrDefault(r => r.Id == id);
        if (reise == null) return;

        // Programmpunkte zuerst entfernen, danach die Reise selbst.
        context.Programmpunkte.RemoveRange(reise.Programmpunkte);
        context.Reisen.Remove(reise);
        context.SaveChanges();
    }
}
