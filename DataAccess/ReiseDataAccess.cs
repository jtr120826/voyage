using Microsoft.EntityFrameworkCore;
using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

public class ReiseDataAccess
{
    public List<Reise> GetAll()
    {
        using var context = new AppDbContext();

        // Include() lädt die Programmpunkte pro Reise gleich mit (Navigation Property) -
        // eine einzelne Abfrage statt pro Reise separat nachzuladen (N+1-Falle vermeiden).
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

    public void Add(Reise reise)
    {
        using var context = new AppDbContext();
        context.Reisen.Add(reise);
        context.SaveChanges();
    }

    public void Delete(int id)
    {
        using var context = new AppDbContext();

        var reise = context.Reisen
            .Include(r => r.Programmpunkte)
            .FirstOrDefault(r => r.Id == id);
        if (reise == null) return;

        // Abhängige Programmpunkte zuerst entfernen (kein automatisches Cascade-Delete konfiguriert)
        context.Programmpunkte.RemoveRange(reise.Programmpunkte);
        context.Reisen.Remove(reise);
        context.SaveChanges();
    }
}
