using Reiseplaner.DataAccess;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

public class ProgrammpunktService
{
    private readonly ProgrammpunktDataAccess _dataAccess = new();

    public List<Programmpunkt> GetByReise(int reiseId) => _dataAccess.GetByReise(reiseId);

    public void Hinzufuegen(Programmpunkt punkt) => _dataAccess.Add(punkt);

    // Business-Entscheidung "was passiert" (Umschalten des Status) gehört in den Service,
    // nicht ins Code-Behind - die UI kennt nur noch den Wunsch "toggle", nicht die Logik dahinter.
    public void ErledigtUmschalten(Programmpunkt punkt) => _dataAccess.SetErledigt(punkt.Id, !punkt.Erledigt);

    public void Loeschen(int id) => _dataAccess.Delete(id);

    // Berechnung (Business-Logik): wie viel einer Reise-Budgets ist durch die Programmpunkte
    // bereits verplant, und ist das Budget überschritten?
    public BudgetStatus BerechneBudgetStatus(Reise reise, List<Programmpunkt> punkte)
    {
        var ausgegeben = punkte.Sum(p => p.Kosten);
        var rest = reise.Budget - ausgegeben;
        return new BudgetStatus(ausgegeben, rest, rest < 0);
    }
}
