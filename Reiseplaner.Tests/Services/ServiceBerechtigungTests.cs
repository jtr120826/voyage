using Reiseplaner.Dtos;
using Reiseplaner.Models;
using Reiseplaner.Services;
using Reiseplaner.Tests.Fakes;
using Reiseplaner.Tests.Hilfsmittel;

namespace Reiseplaner.Tests.Services;

/// <summary>
/// Beweist, dass die Berechtigungen im Service erzwungen werden (OWASP A01 Broken Access Control),
/// auch wenn die Oberfläche eine Schaltfläche fälschlich freigibt oder umgangen wird.
/// </summary>
public class ServiceBerechtigungTests
{
    private static ReiseDto NeueReise() => new()
    {
        Titel = "Rom", Zielort = "Italien", Startdatum = "2026-05-01", Enddatum = "2026-05-07", Budget = 1000m
    };

    private static ProgrammpunktDto NeuerPunkt() => new()
    {
        Titel = "Kolosseum", Datum = "2026-05-02", Kategorie = Kategorien.Alle[0], Kosten = 30m
    };

    // --- Reisen ---

    [Fact]
    public void Reise_NichtAngemeldet_AlleAktionenWerfenUnauthorizedAccessException()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        var service = new ReiseService(repository, new BenutzerSitzung());

        // Act und Assert
        Assert.Throws<UnauthorizedAccessException>(() => service.LadeAlle());
        Assert.Throws<UnauthorizedAccessException>(() => service.Hinzufuegen(NeueReise()));
        Assert.Throws<UnauthorizedAccessException>(() => service.Aktualisieren(NeueReise()));
        Assert.Throws<UnauthorizedAccessException>(() => service.Loeschen(1));
        Assert.Empty(repository.Reisen);
    }

    [Fact]
    public void Reise_Leser_DarfLadenAberNichtAendern()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        repository.Add(new Reise { Titel = "Alt", Zielort = "X", Startdatum = "2026-01-01", Enddatum = "2026-01-02" });
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Leser));

        // Act und Assert
        Assert.Single(service.LadeAlle());
        Assert.Throws<UnauthorizedAccessException>(() => service.Hinzufuegen(NeueReise()));
        Assert.Throws<UnauthorizedAccessException>(() => service.Aktualisieren(new ReiseDto { Id = 1, Titel = "Neu" }));
        Assert.Throws<UnauthorizedAccessException>(() => service.Loeschen(1));
        Assert.Equal("Alt", Assert.Single(repository.Reisen).Titel);
    }

    [Fact]
    public void Reise_Sachbearbeiter_DarfBearbeitenAberNichtLoeschen()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Sachbearbeiter));

        // Act
        service.Hinzufuegen(NeueReise());

        // Assert
        Assert.Single(repository.Reisen);
        Assert.Throws<UnauthorizedAccessException>(() => service.Loeschen(repository.Reisen[0].Id));
        Assert.Single(repository.Reisen);
    }

    [Fact]
    public void Reise_Administrator_DarfLoeschen()
    {
        // Arrange
        var repository = new FakeReiseRepository();
        var service = new ReiseService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        service.Hinzufuegen(NeueReise());

        // Act
        service.Loeschen(repository.Reisen[0].Id);

        // Assert
        Assert.Empty(repository.Reisen);
    }

    [Fact]
    public void Reise_AbgelehnteAktion_LoestKeinDatenGeaendertAus()
    {
        // Arrange
        var service = new ReiseService(new FakeReiseRepository(), TestDaten.SitzungAls(Rolle.Leser));
        var ausgeloest = false;
        service.DatenGeaendert += (_, _) => ausgeloest = true;

        // Act
        Assert.Throws<UnauthorizedAccessException>(() => service.Hinzufuegen(NeueReise()));

        // Assert
        Assert.False(ausgeloest);
    }

    [Fact]
    public void Reise_NachAbmelden_IstDerZugriffGesperrt()
    {
        // Arrange
        var sitzung = TestDaten.SitzungAls(Rolle.Administrator);
        var service = new ReiseService(new FakeReiseRepository(), sitzung);
        service.Hinzufuegen(NeueReise());

        // Act
        sitzung.Abmelden();

        // Assert
        Assert.Throws<UnauthorizedAccessException>(() => service.LadeAlle());
    }

    // --- Programmpunkte ---

    [Fact]
    public void Programmpunkt_NichtAngemeldet_AlleAktionenWerfenUnauthorizedAccessException()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        var service = new ProgrammpunktService(repository, new BenutzerSitzung());

        // Act und Assert
        Assert.Throws<UnauthorizedAccessException>(() => service.LadeProgrammpunkte(1));
        Assert.Throws<UnauthorizedAccessException>(() => service.Hinzufuegen(1, NeuerPunkt()));
        Assert.Throws<UnauthorizedAccessException>(() => service.Aktualisieren(NeuerPunkt()));
        Assert.Throws<UnauthorizedAccessException>(() => service.ErledigtUmschalten(1));
        Assert.Throws<UnauthorizedAccessException>(() => service.Loeschen(1));
        Assert.Empty(repository.Programmpunkte);
    }

    [Fact]
    public void Programmpunkt_Leser_DarfLadenAberNichtAendern()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        repository.Add(new Programmpunkt { ReiseId = 1, Titel = "Alt", Datum = "2026-05-02", Kategorie = Kategorien.Alle[0] });
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Leser));

        // Act und Assert
        Assert.Single(service.LadeProgrammpunkte(1));
        Assert.Throws<UnauthorizedAccessException>(() => service.Hinzufuegen(1, NeuerPunkt()));
        Assert.Throws<UnauthorizedAccessException>(() => service.Aktualisieren(new ProgrammpunktDto { Id = 1, Titel = "Neu" }));
        Assert.Throws<UnauthorizedAccessException>(() => service.ErledigtUmschalten(1));
        Assert.Throws<UnauthorizedAccessException>(() => service.Loeschen(1));

        var gespeichert = Assert.Single(repository.Programmpunkte);
        Assert.Equal("Alt", gespeichert.Titel);
        Assert.False(gespeichert.Erledigt);
    }

    [Fact]
    public void Programmpunkt_Sachbearbeiter_DarfBearbeitenUndErledigenAberNichtLoeschen()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Sachbearbeiter));

        // Act
        service.Hinzufuegen(1, NeuerPunkt());
        var id = repository.Programmpunkte[0].Id;
        service.ErledigtUmschalten(id);

        // Assert
        Assert.True(repository.Programmpunkte[0].Erledigt);
        Assert.Throws<UnauthorizedAccessException>(() => service.Loeschen(id));
        Assert.Single(repository.Programmpunkte);
    }

    [Fact]
    public void Programmpunkt_Administrator_DarfLoeschen()
    {
        // Arrange
        var repository = new FakeProgrammpunktRepository();
        var service = new ProgrammpunktService(repository, TestDaten.SitzungAls(Rolle.Administrator));
        service.Hinzufuegen(1, NeuerPunkt());

        // Act
        service.Loeschen(repository.Programmpunkte[0].Id);

        // Assert
        Assert.Empty(repository.Programmpunkte);
    }
}
