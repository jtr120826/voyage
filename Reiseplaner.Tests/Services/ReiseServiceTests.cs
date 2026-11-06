using Reiseplaner.Dtos;
using Reiseplaner.Models;
using Reiseplaner.Services;
using Reiseplaner.Tests.Fakes;

namespace Reiseplaner.Tests.Services;

public class ReiseServiceTests
{
    [Fact]
    public void LadeAlle_ReiseMitProgrammpunkten_BerechnetGeplantesBudgetAlsKostensumme()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 1000m, kosten: [200m, 300m]));
        var service = new ReiseService(repository);

        // Act
        var reisen = service.LadeAlle();

        // Assert
        var reise = Assert.Single(reisen);
        Assert.Equal(500m, reise.GeplantesBudget);
        Assert.Equal(500m, reise.VerbleibendesBudget);
        Assert.False(reise.IstBudgetUeberschritten);
    }

    [Fact]
    public void LadeAlle_KostenUeberBudget_MeldetBudgetUeberschritten()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: [150m]));
        var service = new ReiseService(repository);

        // Act
        var reise = Assert.Single(service.LadeAlle());

        // Assert
        Assert.Equal(-50m, reise.VerbleibendesBudget);
        Assert.True(reise.IstBudgetUeberschritten, "Kosten über dem Budget müssen als überschritten gelten.");
    }

    [Fact]
    public void LadeAlle_KostenGenauAmBudget_IstNichtUeberschritten()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: [60m, 40m]));
        var service = new ReiseService(repository);

        // Act
        var reise = Assert.Single(service.LadeAlle());

        // Assert
        Assert.Equal(0m, reise.VerbleibendesBudget);
        Assert.False(reise.IstBudgetUeberschritten, "Ein aufgebrauchtes Budget ist noch nicht überschritten.");
    }

    [Fact]
    public void LadeAlle_ReiseOhneProgrammpunkte_GeplantesBudgetIstNull()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 800m, kosten: []));
        var service = new ReiseService(repository);

        // Act
        var reise = Assert.Single(service.LadeAlle());

        // Assert
        Assert.Equal(0m, reise.GeplantesBudget);
        Assert.Equal(800m, reise.VerbleibendesBudget);
    }

    [Fact]
    public void LadeAlle_KeineReisenVorhanden_GibtLeereListeZurueck()
    {
        // Arrange
        var service = new ReiseService(new FakeReiseRepository());

        // Act
        var reisen = service.LadeAlle();

        // Assert
        Assert.Empty(reisen);
    }

    [Fact]
    public void LadeAlle_ReiseMitStammdaten_UebernimmtAlleFelderInsDto()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(new Reise
        {
            Id = 7,
            Titel = "Portugal",
            Zielort = "Lissabon",
            Startdatum = "2026-11-10",
            Enddatum = "2026-11-17",
            Budget = 1200m
        });
        var service = new ReiseService(repository);

        // Act
        var reise = Assert.Single(service.LadeAlle());

        // Assert
        Assert.Equal(7, reise.Id);
        Assert.Equal("Portugal", reise.Titel);
        Assert.Equal("Lissabon", reise.Zielort);
        Assert.Equal("2026-11-10", reise.Startdatum);
        Assert.Equal("2026-11-17", reise.Enddatum);
        Assert.Equal(1200m, reise.Budget);
    }

    [Fact]
    public void Hinzufuegen_GueltigesDto_SpeichertReiseMitDenWertenAusDemDto()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        var service = new ReiseService(repository);
        var dto = new ReiseDto
        {
            Titel = "Japan",
            Zielort = "Tokio",
            Startdatum = "2026-10-05",
            Enddatum = "2026-10-18",
            Budget = 3200m
        };

        // Act
        service.Hinzufuegen(dto);

        // Assert
        var gespeichert = Assert.Single(repository.Reisen);
        Assert.Equal("Japan", gespeichert.Titel);
        Assert.Equal("Tokio", gespeichert.Zielort);
        Assert.Equal("2026-10-05", gespeichert.Startdatum);
        Assert.Equal("2026-10-18", gespeichert.Enddatum);
        Assert.Equal(3200m, gespeichert.Budget);
    }

    [Fact]
    public void Loeschen_ExistierendeReise_EntferntNurDieseReise()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: []));
        repository.Reisen.Add(ErstelleReise(id: 2, budget: 200m, kosten: []));
        var service = new ReiseService(repository);

        // Act
        service.Loeschen(1);

        // Assert
        var uebrig = Assert.Single(repository.Reisen);
        Assert.Equal(2, uebrig.Id);
    }

    [Fact]
    public void Loeschen_NichtExistierendeReise_LaesstBestandUnveraendert()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: []));
        var service = new ReiseService(repository);

        // Act
        var fehler = Record.Exception(() => service.Loeschen(999));

        // Assert
        Assert.Null(fehler);
        Assert.Single(repository.Reisen);
    }

    private static Reise ErstelleReise(int id, decimal budget, decimal[] kosten)
    {
        var reise = new Reise
        {
            Id = id,
            Titel = $"Reise {id}",
            Zielort = "Ort",
            Startdatum = "2026-01-01",
            Enddatum = "2026-01-02",
            Budget = budget
        };

        foreach (var einzelneKosten in kosten)
        {
            reise.Programmpunkte.Add(new Programmpunkt
            {
                Titel = "Programmpunkt",
                Datum = "2026-01-01",
                Kategorie = Kategorien.Essen,
                Kosten = einzelneKosten,
                ReiseId = id
            });
        }

        return reise;
    }
}
