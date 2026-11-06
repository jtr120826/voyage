using Microsoft.EntityFrameworkCore;
using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

/// <summary>
/// Datenzugriff auf Reisen. Lädt die Programmpunkte immer mit und löscht sie beim Löschen einer Reise.
/// </summary>
public class ReiseRepository : Repository<Reise>
{
    /// <summary>
    /// Lädt alle Reisen inklusive ihrer Programmpunkte, sortiert nach Startdatum.
    /// </summary>
    public override List<Reise> GetAll()
    {
        using var context = new AppDbContext();

        // Include() lädt die Programmpunkte gleich mit (eine Abfrage statt N+1).
        // Der ReiseService braucht sie, um das geplante Budget zu berechnen.
        return context.Reisen
            .Include(r => r.Programmpunkte)
            .OrderBy(r => r.Startdatum)
            .ToList();
    }

    /// <summary>
    /// Löscht die Reise samt ihren Programmpunkten.
    /// </summary>
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
