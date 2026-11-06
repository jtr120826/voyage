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
    /// Wird ausgelöst, nachdem sich Programmpunkte geändert haben (hinzugefügt, bearbeitet, erledigt umgeschaltet
    /// oder gelöscht). Auch die Budgetwerte der Reisen ändern sich dadurch.
    /// </summary>
    public event EventHandler? DatenGeaendert;

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
        var programmpunkt = new Programmpunkt { ReiseId = reiseId };
        UebernehmeWerte(programmpunktDto, programmpunkt);

        _repository.Add(programmpunkt);
        MeldeAenderung();
    }

    /// <summary>
    /// Übernimmt Titel, Datum, Kategorie und Kosten aus dem DTO in den bestehenden Programmpunkt
    /// mit derselben Id. Erledigt-Status und Reise bleiben unverändert.
    /// Existiert der Programmpunkt nicht, passiert nichts.
    /// </summary>
    public void Aktualisieren(ProgrammpunktDto programmpunktDto)
    {
        var programmpunkt = _repository.GetById(programmpunktDto.Id);
        if (programmpunkt == null) return;

        UebernehmeWerte(programmpunktDto, programmpunkt);
        _repository.Update(programmpunkt);
        MeldeAenderung();
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
        MeldeAenderung();
    }

    /// <summary>
    /// Löscht den Programmpunkt mit der angegebenen Id.
    /// </summary>
    public void Loeschen(int id)
    {
        _repository.Delete(id);
        MeldeAenderung();
    }

    private void MeldeAenderung() => DatenGeaendert?.Invoke(this, EventArgs.Empty);

    private static void UebernehmeWerte(ProgrammpunktDto programmpunktDto, Programmpunkt programmpunkt)
    {
        programmpunkt.Titel = programmpunktDto.Titel;
        programmpunkt.Datum = programmpunktDto.Datum;
        programmpunkt.Kategorie = programmpunktDto.Kategorie;
        programmpunkt.Kosten = programmpunktDto.Kosten;
    }

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
