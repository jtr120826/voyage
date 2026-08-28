using Reiseplaner.DataAccess;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

public class ReiseService
{
    private readonly ReiseDataAccess _dataAccess = new();

    public List<Reise> GetAlle() => _dataAccess.GetAll();

    public void Hinzufuegen(Reise reise) => _dataAccess.Add(reise);

    public void Loeschen(int id) => _dataAccess.Delete(id);
}
