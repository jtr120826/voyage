using Reiseplaner.DataAccess;
using Reiseplaner.Models;

namespace Reiseplaner.Tests.Fakes;

/// <summary>
/// Speicherinterne Ersatz-Implementierung von <see cref="IProgrammpunktRepository"/>, ganz ohne Datenbank.
/// </summary>
/// <remarks>
/// <see cref="GetById"/> liefert wie die echte Datenbank eine Kopie. Änderungen werden deshalb
/// nur gespeichert, wenn der Service <see cref="Update"/> aufruft.
/// </remarks>
public class FakeProgrammpunktRepository : IProgrammpunktRepository
{
    private int _naechsteId = 1;

    public List<Programmpunkt> Programmpunkte { get; } = new();

    public List<Programmpunkt> GetAll() => Programmpunkte.Select(Kopiere).ToList();

    public Programmpunkt? GetById(int id)
    {
        var gespeichert = Programmpunkte.FirstOrDefault(p => p.Id == id);
        return gespeichert == null ? null : Kopiere(gespeichert);
    }

    public List<Programmpunkt> GetByReise(int reiseId)
    {
        return Programmpunkte
            .Where(p => p.ReiseId == reiseId)
            .OrderBy(p => p.Datum)
            .Select(Kopiere)
            .ToList();
    }

    public void Add(Programmpunkt entity)
    {
        entity.Id = _naechsteId++;
        Programmpunkte.Add(Kopiere(entity));
    }

    public void Update(Programmpunkt entity)
    {
        var index = Programmpunkte.FindIndex(p => p.Id == entity.Id);
        if (index >= 0) Programmpunkte[index] = Kopiere(entity);
    }

    public void Delete(int id) => Programmpunkte.RemoveAll(p => p.Id == id);

    private static Programmpunkt Kopiere(Programmpunkt original)
    {
        return new Programmpunkt
        {
            Id = original.Id,
            Titel = original.Titel,
            Datum = original.Datum,
            Kategorie = original.Kategorie,
            Kosten = original.Kosten,
            Erledigt = original.Erledigt,
            ReiseId = original.ReiseId
        };
    }
}
