using Reiseplaner.DataAccess;
using Reiseplaner.Models;

namespace Reiseplaner.Tests.Fakes;

/// <summary>
/// Speicherinterne Ersatz-Implementierung von <see cref="IRepository{T}"/> für Reisen, ganz ohne Datenbank.
/// </summary>
public class FakeReiseRepository : IRepository<Reise>
{
    private int _naechsteId = 1;

    public List<Reise> Reisen { get; } = new();

    public List<Reise> GetAll() => Reisen.ToList();

    public Reise? GetById(int id) => Reisen.FirstOrDefault(r => r.Id == id);

    public void Add(Reise entity)
    {
        entity.Id = _naechsteId++;
        Reisen.Add(entity);
    }

    public void Update(Reise entity)
    {
        var index = Reisen.FindIndex(r => r.Id == entity.Id);
        if (index >= 0) Reisen[index] = entity;
    }

    public void Delete(int id) => Reisen.RemoveAll(r => r.Id == id);
}
