using Reiseplaner.DataAccess;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

public class ProgrammpunktService
{
    private readonly IProgrammpunktRepository _repository;

    public ProgrammpunktService(IProgrammpunktRepository repository)
    {
        _repository = repository;
    }

    public List<Programmpunkt> GetByReise(int reiseId) => _repository.GetByReise(reiseId);

    public void Hinzufuegen(Programmpunkt punkt) => _repository.Add(punkt);

    public void ErledigtUmschalten(Programmpunkt punkt)
    {
        var gespeicherterPunkt = _repository.GetById(punkt.Id);
        if (gespeicherterPunkt == null) return;

        gespeicherterPunkt.Erledigt = !gespeicherterPunkt.Erledigt;
        _repository.Update(gespeicherterPunkt);
    }

    public void Loeschen(int id) => _repository.Delete(id);

    public BudgetStatus BerechneBudgetStatus(Reise reise, List<Programmpunkt> punkte)
    {
        var ausgegeben = punkte.Sum(p => p.Kosten);
        var rest = reise.Budget - ausgegeben;
        return new BudgetStatus(ausgegeben, rest, rest < 0);
    }
}
