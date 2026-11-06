using Reiseplaner.DataAccess;
using Reiseplaner.Dtos;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

/// <summary>
/// Business-Logik für Reisen: laden, anlegen, löschen und das Budget berechnen.
/// </summary>
public class ReiseService
{
    private readonly IRepository<Reise> _repository;

    public ReiseService(IRepository<Reise> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Wird ausgelöst, nachdem sich die Reisen geändert haben (hinzugefügt, bearbeitet oder gelöscht).
    /// </summary>
    public event EventHandler? DatenGeaendert;

    /// <summary>
    /// Lädt alle Reisen als DTOs, inklusive geplantem und verbleibendem Budget.
    /// </summary>
    public List<ReiseDto> LadeAlle()
    {
        return _repository.GetAll()
            .Select(ErstelleDto)
            .ToList();
    }

    /// <summary>
    /// Legt eine neue Reise an. Eine allfällige Id im DTO wird ignoriert.
    /// </summary>
    public void Hinzufuegen(ReiseDto reiseDto)
    {
        var reise = new Reise();
        UebernehmeWerte(reiseDto, reise);

        _repository.Add(reise);
        MeldeAenderung();
    }

    /// <summary>
    /// Übernimmt die Werte aus dem DTO in die bestehende Reise mit derselben Id.
    /// Existiert die Reise nicht, passiert nichts.
    /// </summary>
    public void Aktualisieren(ReiseDto reiseDto)
    {
        var reise = _repository.GetById(reiseDto.Id);
        if (reise == null) return;

        UebernehmeWerte(reiseDto, reise);
        _repository.Update(reise);
        MeldeAenderung();
    }

    /// <summary>
    /// Löscht die Reise mit der angegebenen Id.
    /// </summary>
    public void Loeschen(int id)
    {
        _repository.Delete(id);
        MeldeAenderung();
    }

    private void MeldeAenderung() => DatenGeaendert?.Invoke(this, EventArgs.Empty);

    private static void UebernehmeWerte(ReiseDto reiseDto, Reise reise)
    {
        reise.Titel = reiseDto.Titel;
        reise.Zielort = reiseDto.Zielort;
        reise.Startdatum = reiseDto.Startdatum;
        reise.Enddatum = reiseDto.Enddatum;
        reise.Budget = reiseDto.Budget;
    }

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
