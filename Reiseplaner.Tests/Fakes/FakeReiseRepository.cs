using Reiseplaner.DataAccess;
using Reiseplaner.Models;

namespace Reiseplaner.Tests.Fakes;

/// <summary>
/// Speicherinterne Ersatz-Implementierung von <see cref="IRepository{T}"/> für Reisen, ganz ohne Datenbank.
/// </summary>
/// <remarks>
/// <see cref="GetById"/> liefert wie die echte Datenbank eine Kopie ohne Programmpunkte. Änderungen werden
/// deshalb nur gespeichert, wenn der Service <see cref="Update"/> aufruft, und die Programmpunkte bleiben erhalten.
/// </remarks>
public class FakeReiseRepository : IRepository<Reise>
{
    private int _naechsteId = 1;

    public List<Reise> Reisen { get; } = new();

    public List<Reise> GetAll() => Reisen.ToList();

    public Reise? GetById(int id)
    {
        var gespeichert = Reisen.FirstOrDefault(r => r.Id == id);
        if (gespeichert == null) return null;

        var kopie = new Reise { Id = gespeichert.Id };
        UebernehmeWerte(gespeichert, kopie);
        return kopie;
    }

    public void Add(Reise entity)
    {
        entity.Id = _naechsteId++;
        Reisen.Add(entity);
    }

    public void Update(Reise entity)
    {
        var gespeichert = Reisen.FirstOrDefault(r => r.Id == entity.Id);
        if (gespeichert != null) UebernehmeWerte(entity, gespeichert);
    }

    public void Delete(int id) => Reisen.RemoveAll(r => r.Id == id);

    private static void UebernehmeWerte(Reise von, Reise nach)
    {
        nach.Titel = von.Titel;
        nach.Zielort = von.Zielort;
        nach.Startdatum = von.Startdatum;
        nach.Enddatum = von.Enddatum;
        nach.Budget = von.Budget;
    }
}
