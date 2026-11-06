using Reiseplaner.DataAccess;
using Reiseplaner.Dtos;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

/// <summary>
/// Business-Logik für Programmpunkte: laden, anlegen, löschen und den Erledigt-Status umschalten.
/// </summary>
public class ProgrammpunktService
{
    private readonly IProgrammpunktRepository _repository;

    public ProgrammpunktService(IProgrammpunktRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Lädt alle Programmpunkte einer Reise als DTOs.
    /// </summary>
    public List<ProgrammpunktDto> LadeProgrammpunkte(int reiseId)
    {
        return _repository.GetByReise(reiseId)
            .Select(ErstelleDto)
            .ToList();
    }

    /// <summary>
    /// Legt einen neuen Programmpunkt für die angegebene Reise an.
    /// </summary>
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

    /// <summary>
    /// Schaltet einen offenen Programmpunkt auf erledigt und einen erledigten wieder auf offen.
    /// Existiert der Programmpunkt nicht, passiert nichts.
    /// </summary>
    public void ErledigtUmschalten(int programmpunktId)
    {
        var programmpunkt = _repository.GetById(programmpunktId);
        if (programmpunkt == null) return;

        programmpunkt.Erledigt = !programmpunkt.Erledigt;
        _repository.Update(programmpunkt);
    }

    /// <summary>
    /// Löscht den Programmpunkt mit der angegebenen Id.
    /// </summary>
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
