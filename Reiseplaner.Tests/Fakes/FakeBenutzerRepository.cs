using Reiseplaner.DataAccess;
using Reiseplaner.Models;

namespace Reiseplaner.Tests.Fakes;

/// <summary>
/// Speicherinterne Ersatz-Implementierung von <see cref="IBenutzerRepository"/>, ganz ohne Datenbank.
/// </summary>
public class FakeBenutzerRepository : IBenutzerRepository
{
    private int _naechsteId = 1;

    public List<Benutzer> Benutzer { get; } = new();

    public List<Benutzer> GetAll() => Benutzer.ToList();

    public Benutzer? GetById(int id) => Benutzer.FirstOrDefault(b => b.Id == id);

    public Benutzer? GetByBenutzername(string benutzername) =>
        Benutzer.FirstOrDefault(b => b.Benutzername == benutzername);

    public void Add(Benutzer entity)
    {
        entity.Id = _naechsteId++;
        Benutzer.Add(entity);
    }

    public void Update(Benutzer entity)
    {
        // Objekte liegen im Speicher, Änderungen sind schon übernommen.
    }

    public void Delete(int id) => Benutzer.RemoveAll(b => b.Id == id);
}
