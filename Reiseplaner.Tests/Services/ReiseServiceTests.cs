using Reiseplaner.Dtos;
using Reiseplaner.Models;
using Reiseplaner.Services;
using Reiseplaner.Tests.Fakes;
using Reiseplaner.Tests.Hilfsmittel;

namespace Reiseplaner.Tests.Services;

public class ReiseServiceTests
{
    [Fact]
    public void LadeAlle_ReiseMitProgrammpunkten_BerechnetGeplantesBudgetAlsKostensumme()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 1000m, kosten: [200m, 300m]));
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));

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
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));

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
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));

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
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));

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
        var service = new ReiseService(new FakeReiseRepository(), TestDaten.SitzungAls(Rolle.Administrator));

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
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));

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
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));
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
    public void Aktualisieren_ExistierendeReise_UebernimmtDieNeuenWerte()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: []));
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ReiseDto
        {
            Id = 1,
            Titel = "Neuer Titel",
            Zielort = "Neuer Ort",
            Startdatum = "2027-02-01",
            Enddatum = "2027-02-08",
            Budget = 999m
        };

        // Act
        service.Aktualisieren(dto);

        // Assert
        var gespeichert = Assert.Single(repository.Reisen);
        Assert.Equal("Neuer Titel", gespeichert.Titel);
        Assert.Equal("Neuer Ort", gespeichert.Zielort);
        Assert.Equal("2027-02-01", gespeichert.Startdatum);
        Assert.Equal("2027-02-08", gespeichert.Enddatum);
        Assert.Equal(999m, gespeichert.Budget);
    }

    [Fact]
    public void Aktualisieren_ReiseMitProgrammpunkten_BehaeltDieProgrammpunkte()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: [30m, 20m]));
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ReiseDto { Id = 1, Titel = "Geändert", Zielort = "Ort", Startdatum = "2026-01-01", Enddatum = "2026-01-02", Budget = 500m };

        // Act
        service.Aktualisieren(dto);

        // Assert
        var reise = Assert.Single(service.LadeAlle());
        Assert.Equal(50m, reise.GeplantesBudget);
        Assert.Equal(450m, reise.VerbleibendesBudget);
    }

    [Fact]
    public void Aktualisieren_ZweiReisen_AendertNurDieAngegebene()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: []));
        repository.Reisen.Add(ErstelleReise(id: 2, budget: 200m, kosten: []));
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ReiseDto { Id = 2, Titel = "Geändert", Zielort = "Ort", Startdatum = "2026-01-01", Enddatum = "2026-01-02", Budget = 250m };

        // Act
        service.Aktualisieren(dto);

        // Assert
        Assert.Equal("Reise 1", repository.Reisen.Single(r => r.Id == 1).Titel);
        Assert.Equal("Geändert", repository.Reisen.Single(r => r.Id == 2).Titel);
    }

    [Fact]
    public void Aktualisieren_NichtExistierendeId_AendertNichtsUndWirftKeinenFehler()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: []));
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ReiseDto { Id = 999, Titel = "Gibt es nicht" };

        // Act
        var fehler = Record.Exception(() => service.Aktualisieren(dto));

        // Assert
        Assert.Null(fehler);
        var unveraendert = Assert.Single(repository.Reisen);
        Assert.Equal("Reise 1", unveraendert.Titel);
    }

    [Fact]
    public void Loeschen_ExistierendeReise_EntferntNurDieseReise()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: []));
        repository.Reisen.Add(ErstelleReise(id: 2, budget: 200m, kosten: []));
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));

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
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        var fehler = Record.Exception(() => service.Loeschen(999));

        // Assert
        Assert.Null(fehler);
        Assert.Single(repository.Reisen);
    }

    [Fact]
    public void Hinzufuegen_GueltigesDto_LoestDatenGeaendertAus()
    {
        // Arrange
        var service = new ReiseService(new FakeReiseRepository(), TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.Hinzufuegen(new ReiseDto { Titel = "Japan" });

        // Assert
        Assert.Equal(1, anzahlMeldungen);
    }

    [Fact]
    public void Aktualisieren_ExistierendeReise_LoestDatenGeaendertAus()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: []));
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.Aktualisieren(new ReiseDto { Id = 1, Titel = "Geändert" });

        // Assert
        Assert.Equal(1, anzahlMeldungen);
    }

    [Fact]
    public void Aktualisieren_NichtExistierendeId_LoestKeinDatenGeaendertAus()
    {
        // Arrange
        var service = new ReiseService(new FakeReiseRepository(), TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.Aktualisieren(new ReiseDto { Id = 999 });

        // Assert
        Assert.Equal(0, anzahlMeldungen);
    }

    [Fact]
    public void Loeschen_ExistierendeReise_LoestDatenGeaendertAus()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Reisen.Add(ErstelleReise(id: 1, budget: 100m, kosten: []));
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.Loeschen(1);

        // Assert
        Assert.Equal(1, anzahlMeldungen);
    }

    [Fact]
    public void DatenGeaendert_ZweiUnabhaengigeAbonnenten_WerdenBeideBenachrichtigt()
    {
        // Arrange
        var service = new ReiseService(new FakeReiseRepository(), TestDaten.SitzungAls(Rolle.Administrator));
        var erster = 0;
        var zweiter = 0;
        service.DatenGeaendert += (_, _) => erster++;
        service.DatenGeaendert += (_, _) => zweiter++;

        // Act
        service.Hinzufuegen(new ReiseDto { Titel = "Japan" });

        // Assert
        Assert.Equal(1, erster);
        Assert.Equal(1, zweiter);
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
