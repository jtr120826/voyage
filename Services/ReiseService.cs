using Reiseplaner.DataAccess;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

public class ReiseService
{
    private readonly IRepository<Reise> _repository;

    public ReiseService(IRepository<Reise> repository)
    {
        _repository = repository;
    }

    public List<Reise> GetAlle() => _repository.GetAll();

    public void Hinzufuegen(Reise reise) => _repository.Add(reise);

    public void Loeschen(int id) => _repository.Delete(id);
}
