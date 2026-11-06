using Reiseplaner.Dtos;
using Reiseplaner.Models;
using Reiseplaner.Services;
using Reiseplaner.Tests.Fakes;
using Reiseplaner.Tests.Hilfsmittel;

namespace Reiseplaner.Tests.Services;

public class ProgrammpunktServiceTests
{
    [Fact]
    public void ErledigtUmschalten_OffenerProgrammpunkt_WirdErledigt()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        service.ErledigtUmschalten(1);

        // Assert
        var gespeichert = Assert.Single(repository.Programmpunkte);
        Assert.True(gespeichert.Erledigt, "Ein offener Programmpunkt muss nach dem Umschalten erledigt sein.");
    }

    [Fact]
    public void ErledigtUmschalten_ErledigterProgrammpunkt_WirdWiederOffen()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: true));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        service.ErledigtUmschalten(1);

        // Assert
        var gespeichert = Assert.Single(repository.Programmpunkte);
        Assert.False(gespeichert.Erledigt, "Ein erledigter Programmpunkt muss nach dem Umschalten wieder offen sein.");
    }

    [Fact]
    public void ErledigtUmschalten_ZweiMalUmgeschaltet_IstWiederImUrsprungszustand()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        service.ErledigtUmschalten(1);
        service.ErledigtUmschalten(1);

        // Assert
        Assert.False(Assert.Single(repository.Programmpunkte).Erledigt);
    }

    [Fact]
    public void ErledigtUmschalten_ZweiProgrammpunkte_AendertNurDenAngegebenen()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 2, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        service.ErledigtUmschalten(2);

        // Assert
        Assert.False(repository.Programmpunkte.Single(p => p.Id == 1).Erledigt);
        Assert.True(repository.Programmpunkte.Single(p => p.Id == 2).Erledigt);
    }

    [Fact]
    public void ErledigtUmschalten_NichtExistierendeId_AendertNichtsUndWirftKeinenFehler()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        var fehler = Record.Exception(() => service.ErledigtUmschalten(999));

        // Assert
        Assert.Null(fehler);
        Assert.False(Assert.Single(repository.Programmpunkte).Erledigt);
    }

    [Fact]
    public void LadeProgrammpunkte_ReiseMitProgrammpunkten_GibtNurDieseReiseZurueck()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 2, reiseId: 2, erledigt: false));
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 3, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        var programmpunkte = service.LadeProgrammpunkte(1);

        // Assert
        Assert.Equal([1, 3], programmpunkte.Select(p => p.Id));
    }

    [Fact]
    public void LadeProgrammpunkte_ReiseOhneProgrammpunkte_GibtLeereListeZurueck()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        var programmpunkte = service.LadeProgrammpunkte(42);

        // Assert
        Assert.Empty(programmpunkte);
    }

    [Fact]
    public void LadeProgrammpunkte_ProgrammpunktMitAllenFeldern_UebernimmtAlleFelderInsDto()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(new Programmpunkt
        {
            Id = 5,
            Titel = "Sintra Ausflug",
            Datum = "2026-11-12",
            Kategorie = Kategorien.Sehenswuerdigkeit,
            Kosten = 80m,
            Erledigt = true,
            ReiseId = 1
        });
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        var programmpunkt = Assert.Single(service.LadeProgrammpunkte(1));

        // Assert
        Assert.Equal(5, programmpunkt.Id);
        Assert.Equal("Sintra Ausflug", programmpunkt.Titel);
        Assert.Equal("2026-11-12", programmpunkt.Datum);
        Assert.Equal(Kategorien.Sehenswuerdigkeit, programmpunkt.Kategorie);
        Assert.Equal(80m, programmpunkt.Kosten);
        Assert.True(programmpunkt.Erledigt);
    }

    [Fact]
    public void Hinzufuegen_GueltigesDto_SpeichertProgrammpunktFuerDieAngegebeneReise()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ProgrammpunktDto
        {
            Titel = "Zug nach Porto",
            Datum = "2026-11-14",
            Kategorie = Kategorien.Transport,
            Kosten = 45m
        };

        // Act
        service.Hinzufuegen(3, dto);

        // Assert
        var gespeichert = Assert.Single(repository.Programmpunkte);
        Assert.Equal(3, gespeichert.ReiseId);
        Assert.Equal("Zug nach Porto", gespeichert.Titel);
        Assert.Equal("2026-11-14", gespeichert.Datum);
        Assert.Equal(Kategorien.Transport, gespeichert.Kategorie);
        Assert.Equal(45m, gespeichert.Kosten);
        Assert.False(gespeichert.Erledigt, "Ein neuer Programmpunkt ist noch nicht erledigt.");
    }

    [Fact]
    public void Aktualisieren_ExistierenderProgrammpunkt_UebernimmtDieNeuenWerte()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ProgrammpunktDto
        {
            Id = 1,
            Titel = "Neuer Titel",
            Datum = "2027-03-03",
            Kategorie = Kategorien.Unterkunft,
            Kosten = 250m
        };

        // Act
        service.Aktualisieren(dto);

        // Assert
        var gespeichert = Assert.Single(repository.Programmpunkte);
        Assert.Equal("Neuer Titel", gespeichert.Titel);
        Assert.Equal("2027-03-03", gespeichert.Datum);
        Assert.Equal(Kategorien.Unterkunft, gespeichert.Kategorie);
        Assert.Equal(250m, gespeichert.Kosten);
    }

    [Fact]
    public void Aktualisieren_ErledigterProgrammpunkt_BehaeltErledigtStatusUndReise()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 7, erledigt: true));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ProgrammpunktDto { Id = 1, Titel = "Geändert", Datum = "2026-01-01", Kategorie = Kategorien.Essen, Kosten = 5m, Erledigt = false };

        // Act
        service.Aktualisieren(dto);

        // Assert
        var gespeichert = Assert.Single(repository.Programmpunkte);
        Assert.True(gespeichert.Erledigt, "Das Bearbeiten darf den Erledigt-Status nicht zurücksetzen.");
        Assert.Equal(7, gespeichert.ReiseId);
    }

    [Fact]
    public void Aktualisieren_ZweiProgrammpunkte_AendertNurDenAngegebenen()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 2, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ProgrammpunktDto { Id = 2, Titel = "Geändert", Datum = "2026-01-01", Kategorie = Kategorien.Essen, Kosten = 10m };

        // Act
        service.Aktualisieren(dto);

        // Assert
        Assert.Equal("Programmpunkt 1", repository.Programmpunkte.Single(p => p.Id == 1).Titel);
        Assert.Equal("Geändert", repository.Programmpunkte.Single(p => p.Id == 2).Titel);
    }

    [Fact]
    public void Aktualisieren_NichtExistierendeId_AendertNichtsUndWirftKeinenFehler()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var dto = new ProgrammpunktDto { Id = 999, Titel = "Gibt es nicht" };

        // Act
        var fehler = Record.Exception(() => service.Aktualisieren(dto));

        // Assert
        Assert.Null(fehler);
        Assert.Equal("Programmpunkt 1", Assert.Single(repository.Programmpunkte).Titel);
    }

    [Fact]
    public void Loeschen_ExistierenderProgrammpunkt_EntferntNurDiesen()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 2, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        service.Loeschen(1);

        // Assert
        Assert.Equal(2, Assert.Single(repository.Programmpunkte).Id);
    }

    [Fact]
    public void Loeschen_NichtExistierendeId_LaesstBestandUnveraendert()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));

        // Act
        var fehler = Record.Exception(() => service.Loeschen(999));

        // Assert
        Assert.Null(fehler);
        Assert.Single(repository.Programmpunkte);
    }

    [Fact]
    public void Hinzufuegen_GueltigesDto_LoestDatenGeaendertAus()
    {
        // Arrange
        var service = new ProgrammpunktService(new FakeProgrammpunktRepository(), TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.Hinzufuegen(1, new ProgrammpunktDto { Titel = "Zug nach Porto" });

        // Assert
        Assert.Equal(1, anzahlMeldungen);
    }

    [Fact]
    public void Aktualisieren_ExistierenderProgrammpunkt_LoestDatenGeaendertAus()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.Aktualisieren(new ProgrammpunktDto { Id = 1, Titel = "Geändert" });

        // Assert
        Assert.Equal(1, anzahlMeldungen);
    }

    [Fact]
    public void ErledigtUmschalten_ExistierenderProgrammpunkt_LoestDatenGeaendertAus()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.ErledigtUmschalten(1);

        // Assert
        Assert.Equal(1, anzahlMeldungen);
    }

    [Fact]
    public void ErledigtUmschalten_NichtExistierendeId_LoestKeinDatenGeaendertAus()
    {
        // Arrange
        var service = new ProgrammpunktService(new FakeProgrammpunktRepository(), TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.ErledigtUmschalten(999);

        // Assert
        Assert.Equal(0, anzahlMeldungen);
    }

    [Fact]
    public void Loeschen_ExistierenderProgrammpunkt_LoestDatenGeaendertAus()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Programmpunkte.Add(ErstelleProgrammpunkt(id: 1, reiseId: 1, erledigt: false));
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        var anzahlMeldungen = 0;
        service.DatenGeaendert += (_, _) => anzahlMeldungen++;

        // Act
        service.Loeschen(1);

        // Assert
        Assert.Equal(1, anzahlMeldungen);
    }

    private static Programmpunkt ErstelleProgrammpunkt(int id, int reiseId, bool erledigt)
    {
        return new Programmpunkt
        {
            Id = id,
            Titel = $"Programmpunkt {id}",
            Datum = "2026-01-01",
            Kategorie = Kategorien.Essen,
            Kosten = 10m,
            Erledigt = erledigt,
            ReiseId = reiseId
        };
    }
}
