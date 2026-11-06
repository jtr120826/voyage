using Reiseplaner.DataAccess;
using Reiseplaner.Dtos;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

public class ReiseService
{
    private readonly IRepository<Reise> _repository;

    public ReiseService(IRepository<Reise> repository)
    {
        _repository = repository;
    }

    public List<ReiseDto> GetAlle()
    {
        return _repository.GetAll()
            .Select(ErstelleDto)
            .ToList();
    }

    public void Hinzufuegen(ReiseDto reiseDto)
    {
        var reise = new Reise
        {
            Titel = reiseDto.Titel,
            Zielort = reiseDto.Zielort,
            Startdatum = reiseDto.Startdatum,
            Enddatum = reiseDto.Enddatum,
            Budget = reiseDto.Budget
        };

        _repository.Add(reise);
    }

    public void Loeschen(int id) => _repository.Delete(id);

    private static ReiseDto ErstelleDto(Reise reise)
    {
        var geplantesBudget = reise.Programmpunkte.Sum(p => p.Kosten);
        var verbleibendesBudget = reise.Budget - geplantesBudget;

        return new ReiseDto
        {
            Id = reise.Id,
            Titel = reise.Titel,
            Zielort = reise.Zielort,
            Startdatum = reise.Startdatum,
            Enddatum = reise.Enddatum,
            Budget = reise.Budget,
            GeplantesBudget = geplantesBudget,
            VerbleibendesBudget = verbleibendesBudget,
            IstBudgetUeberschritten = verbleibendesBudget < 0
        };
    }
}
