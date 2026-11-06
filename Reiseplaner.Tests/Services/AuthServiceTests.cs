using Reiseplaner.Dtos;
using Reiseplaner.Models;
using Reiseplaner.Services;
using Reiseplaner.Tests.Fakes;
using Reiseplaner.Tests.Hilfsmittel;

namespace Reiseplaner.Tests.Services;

public class AuthServiceTests
{
    private readonly FakeBenutzerRepository _repository = new();
    private readonly BenutzerSitzung _sitzung = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        // Wenige Iterationen, damit die Tests schnell laufen.
        _service = new AuthService(_repository, new PasswortHasher(iterationen: 1000), _sitzung);
    }

    [Fact]
    public void GibtEsBenutzer_OhneKonten_GibtFalseZurueck()
    {
        Assert.False(_service.GibtEsBenutzer());
    }

    [Fact]
    public void ErstelleErstenAdministrator_OhneKonten_LegtAdministratorAn()
    {
        // Act
        _service.ErstelleErstenAdministrator("Admin", TestDaten.ZufaelligesPasswort());

        // Assert
        var gespeichert = Assert.Single(_repository.Benutzer);
        Assert.Equal("admin", gespeichert.Benutzername);
        Assert.Equal(Rolle.Administrator, gespeichert.Rolle);
    }

    [Fact]
    public void ErstelleErstenAdministrator_MitBestehendemKonto_WirftInvalidOperationException()
    {
        // Arrange
        _service.ErstelleErstenAdministrator("admin", TestDaten.ZufaelligesPasswort());

        // Act und Assert
        Assert.Throws<InvalidOperationException>(
            () => _service.ErstelleErstenAdministrator("zweiter", TestDaten.ZufaelligesPasswort()));
        Assert.Single(_repository.Benutzer);
    }

    [Fact]
    public void ErstelleErstenAdministrator_SpeichertKeinKlartextPasswort()
    {
        // Arrange
        var passwort = TestDaten.ZufaelligesPasswort();

        // Act
        _service.ErstelleErstenAdministrator("admin", passwort);

        // Assert
        var gespeichert = Assert.Single(_repository.Benutzer);
        Assert.NotEqual(passwort, gespeichert.PasswortHash);
        Assert.DoesNotContain(passwort, gespeichert.PasswortHash);
        Assert.DoesNotContain(passwort, gespeichert.Salt);
        Assert.Equal(1000, gespeichert.Iterationen);
    }

    [Fact]
    public void ErstelleBenutzer_GleichesPasswortFuerZweiKonten_ErgibtVerschiedeneHashes()
    {
        // Arrange
        var passwort = TestDaten.ZufaelligesPasswort();
        _service.ErstelleErstenAdministrator("admin", passwort);
        _service.MeldeAn("admin", passwort);

        // Act
        _service.ErstelleBenutzer("zweiter", passwort, Rolle.Leser);

        // Assert
        Assert.NotEqual(_repository.Benutzer[0].PasswortHash, _repository.Benutzer[1].PasswortHash);
        Assert.NotEqual(_repository.Benutzer[0].Salt, _repository.Benutzer[1].Salt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ErstelleErstenAdministrator_LeererBenutzername_WirftArgumentException(string name)
    {
        Assert.Throws<ArgumentException>(
            () => _service.ErstelleErstenAdministrator(name, TestDaten.ZufaelligesPasswort()));
        Assert.Empty(_repository.Benutzer);
    }

    [Fact]
    public void ErstelleErstenAdministrator_ZuKurzesPasswort_WirftArgumentException()
    {
        var zuKurz = new string('x', AuthService.MindestlaengePasswort - 1);

        Assert.Throws<ArgumentException>(() => _service.ErstelleErstenAdministrator("admin", zuKurz));
        Assert.Empty(_repository.Benutzer);
    }

    [Fact]
    public void ErstelleBenutzer_DoppelterNameAuchMitAndererSchreibweise_WirftArgumentException()
    {
        // Arrange
        var passwort = TestDaten.ZufaelligesPasswort();
        _service.ErstelleErstenAdministrator("admin", passwort);
        _service.MeldeAn("admin", passwort);

        // Act und Assert
        Assert.Throws<ArgumentException>(
            () => _service.ErstelleBenutzer(" ADMIN ", TestDaten.ZufaelligesPasswort(), Rolle.Leser));
        Assert.Single(_repository.Benutzer);
    }

    [Fact]
    public void ErstelleBenutzer_AlsAdministrator_LegtBenutzerMitGewaehlterRolleAn()
    {
        // Arrange
        var passwort = TestDaten.ZufaelligesPasswort();
        _service.ErstelleErstenAdministrator("admin", passwort);
        _service.MeldeAn("admin", passwort);

        // Act
        _service.ErstelleBenutzer("anna", TestDaten.ZufaelligesPasswort(), Rolle.Sachbearbeiter);

        // Assert
        Assert.Equal(Rolle.Sachbearbeiter, _repository.GetByBenutzername("anna")?.Rolle);
    }

    [Theory]
    [InlineData(Rolle.Leser)]
    [InlineData(Rolle.Sachbearbeiter)]
    public void ErstelleBenutzer_OhneAdministratorRolle_WirftUnauthorizedAccessException(Rolle rolle)
    {
        // Arrange
        _service.ErstelleErstenAdministrator("admin", TestDaten.ZufaelligesPasswort());
        _sitzung.Anmelden(new BenutzerDto { Id = 99, Benutzername = "x", Rolle = rolle });

        // Act und Assert
        Assert.Throws<UnauthorizedAccessException>(
            () => _service.ErstelleBenutzer("neu", TestDaten.ZufaelligesPasswort(), Rolle.Administrator));
        Assert.Single(_repository.Benutzer);
    }

    [Fact]
    public void ErstelleBenutzer_NichtAngemeldet_WirftUnauthorizedAccessException()
    {
        // Arrange
        _service.ErstelleErstenAdministrator("admin", TestDaten.ZufaelligesPasswort());

        // Act und Assert
        Assert.Throws<UnauthorizedAccessException>(
            () => _service.ErstelleBenutzer("neu", TestDaten.ZufaelligesPasswort(), Rolle.Leser));
        Assert.Single(_repository.Benutzer);
    }

    [Fact]
    public void MeldeAn_RichtigeDaten_GibtBenutzerZurueckUndSetztSitzung()
    {
        // Arrange
        var passwort = TestDaten.ZufaelligesPasswort();
        _service.ErstelleErstenAdministrator("admin", passwort);

        // Act
        var benutzer = _service.MeldeAn("  Admin ", passwort);

        // Assert
        Assert.NotNull(benutzer);
        Assert.Equal(Rolle.Administrator, benutzer.Rolle);
        Assert.Equal("admin", _sitzung.AngemeldeterBenutzer?.Benutzername);
    }

    [Fact]
    public void MeldeAn_FalschesPasswort_GibtNullZurueckUndMeldetNichtAn()
    {
        // Arrange
        _service.ErstelleErstenAdministrator("admin", TestDaten.ZufaelligesPasswort());

        // Act
        var benutzer = _service.MeldeAn("admin", TestDaten.ZufaelligesPasswort());

        // Assert
        Assert.Null(benutzer);
        Assert.Null(_sitzung.AngemeldeterBenutzer);
    }

    [Fact]
    public void MeldeAn_UnbekannterBenutzer_GibtNullZurueck()
    {
        Assert.Null(_service.MeldeAn("gibtesnicht", TestDaten.ZufaelligesPasswort()));
        Assert.Null(_sitzung.AngemeldeterBenutzer);
    }

    [Fact]
    public void MeldeAn_LeeresPasswort_GibtNullZurueck()
    {
        _service.ErstelleErstenAdministrator("admin", TestDaten.ZufaelligesPasswort());

        Assert.Null(_service.MeldeAn("admin", ""));
    }

    [Fact]
    public void MeldeAb_NachAnmeldung_LeertDieSitzung()
    {
        // Arrange
        var passwort = TestDaten.ZufaelligesPasswort();
        _service.ErstelleErstenAdministrator("admin", passwort);
        _service.MeldeAn("admin", passwort);

        // Act
        _service.MeldeAb();

        // Assert
        Assert.Null(_sitzung.AngemeldeterBenutzer);
    }
}
