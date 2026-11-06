using Reiseplaner.Dtos;
using Reiseplaner.Models;
using Reiseplaner.Services;
using Reiseplaner.Tests.Hilfsmittel;

namespace Reiseplaner.Tests.Services;

public class BenutzerSitzungTests
{
    [Fact]
    public void Pruefe_NiemandAngemeldet_WirftUnauthorizedAccessException()
    {
        // Arrange
        var sitzung = new BenutzerSitzung();

        // Act und Assert
        Assert.Throws<UnauthorizedAccessException>(() => sitzung.Pruefe(Recht.Ansehen));
    }

    [Fact]
    public void Darf_NiemandAngemeldet_GibtFalseZurueck()
    {
        // Arrange
        var sitzung = new BenutzerSitzung();

        // Act und Assert
        Assert.False(sitzung.Darf(Recht.Ansehen), "Ohne Anmeldung darf niemand etwas.");
    }

    [Fact]
    public void Pruefe_RolleOhneBerechtigung_WirftUnauthorizedAccessException()
    {
        // Arrange
        var sitzung = TestDaten.SitzungAls(Rolle.Leser);

        // Act und Assert
        Assert.Throws<UnauthorizedAccessException>(() => sitzung.Pruefe(Recht.Loeschen));
    }

    [Fact]
    public void Pruefe_RolleMitBerechtigung_WirftKeinenFehler()
    {
        // Arrange
        var sitzung = TestDaten.SitzungAls(Rolle.Administrator);

        // Act
        var fehler = Record.Exception(() => sitzung.Pruefe(Recht.Loeschen));

        // Assert
        Assert.Null(fehler);
    }

    [Theory]
    [InlineData(Rolle.Leser, Recht.Ansehen, true)]
    [InlineData(Rolle.Leser, Recht.Bearbeiten, false)]
    [InlineData(Rolle.Leser, Recht.Loeschen, false)]
    [InlineData(Rolle.Leser, Recht.BenutzerVerwalten, false)]
    [InlineData(Rolle.Sachbearbeiter, Recht.Ansehen, true)]
    [InlineData(Rolle.Sachbearbeiter, Recht.Bearbeiten, true)]
    [InlineData(Rolle.Sachbearbeiter, Recht.Loeschen, false)]
    [InlineData(Rolle.Sachbearbeiter, Recht.BenutzerVerwalten, false)]
    [InlineData(Rolle.Administrator, Recht.Ansehen, true)]
    [InlineData(Rolle.Administrator, Recht.Bearbeiten, true)]
    [InlineData(Rolle.Administrator, Recht.Loeschen, true)]
    [InlineData(Rolle.Administrator, Recht.BenutzerVerwalten, true)]
    public void Darf_JeRolleUndRecht_EntsprichtDerBerechtigungstabelle(Rolle rolle, Recht recht, bool erwartet)
    {
        // Arrange
        var sitzung = TestDaten.SitzungAls(rolle);

        // Act
        var darf = sitzung.Darf(recht);

        // Assert
        Assert.Equal(erwartet, darf);
    }

    [Fact]
    public void Abmelden_NachAnmeldung_EntziehtAlleRechte()
    {
        // Arrange
        var sitzung = TestDaten.SitzungAls(Rolle.Administrator);

        // Act
        sitzung.Abmelden();

        // Assert
        Assert.Null(sitzung.AngemeldeterBenutzer);
        Assert.False(sitzung.Darf(Recht.Ansehen));
    }

    [Fact]
    public void Anmelden_ZweiterBenutzer_ErsetztDenErsten()
    {
        // Arrange
        var sitzung = TestDaten.SitzungAls(Rolle.Administrator);

        // Act
        sitzung.Anmelden(new BenutzerDto { Id = 2, Benutzername = "leser", Rolle = Rolle.Leser });

        // Assert
        Assert.Equal("leser", sitzung.AngemeldeterBenutzer?.Benutzername);
        Assert.False(sitzung.Darf(Recht.Loeschen));
    }
}
