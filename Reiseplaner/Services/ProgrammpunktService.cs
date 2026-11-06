using Reiseplaner.DataAccess;
using Reiseplaner.Dtos;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

public class ProgrammpunktService
{
    private readonly IProgrammpunktRepository _repository;

    public ProgrammpunktService(IProgrammpunktRepository repository)
    {
        _repository = repository;
    }

    public List<ProgrammpunktDto> GetByReise(int reiseId)
    {
        return _repository.GetByReise(reiseId)
            .Select(ErstelleDto)
            .ToList();
    }

    public void Hinzufuegen(int reiseId, ProgrammpunktDto programmpunktDto)
    {
        var programmpunkt = new Programmpunkt
        {
            Titel = programmpunktDto.Titel,
            Datum = programmpunktDto.Datum,
            Kategorie = programmpunktDto.Kategorie,
            Kosten = programmpunktDto.Kosten,
            ReiseId = reiseId
        };

        _repository.Add(programmpunkt);
    }

    public void ErledigtUmschalten(int programmpunktId)
    {
        var programmpunkt = _repository.GetById(programmpunktId);
        if (programmpunkt == null) return;

        programmpunkt.Erledigt = !programmpunkt.Erledigt;
        _repository.Update(programmpunkt);
    }

    public void Loeschen(int id) => _repository.Delete(id);

    private static ProgrammpunktDto ErstelleDto(Programmpunkt programmpunkt)
    {
        return new ProgrammpunktDto
        {
            Id = programmpunkt.Id,
            Titel = programmpunkt.Titel,
            Datum = programmpunkt.Datum,
            Kategorie = programmpunkt.Kategorie,
            Kosten = programmpunkt.Kosten,
            Erledigt = programmpunkt.Erledigt
        };
    }
}
