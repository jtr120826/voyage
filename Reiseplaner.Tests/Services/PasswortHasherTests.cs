using Reiseplaner.Services;
using Reiseplaner.Tests.Hilfsmittel;

namespace Reiseplaner.Tests.Services;

public class PasswortHasherTests
{
    // Nur für die Tests: wenige Iterationen, damit sie schnell laufen. Produktiv gilt StandardIterationen.
    private const int SchnelleIterationen = 1000;

    [Fact]
    public void Hashe_Passwort_GibtNichtDasKlartextPasswortZurueck()
    {
        // Arrange
        var hasher = new PasswortHasher(SchnelleIterationen);
        var passwort = TestDaten.ZufaelligesPasswort();

        // Act
        var (hash, salt) = hasher.Hashe(passwort);

        // Assert
        Assert.NotEqual(passwort, hash);
        Assert.DoesNotContain(passwort, hash);
        Assert.DoesNotContain(passwort, salt);
    }

    [Fact]
    public void Hashe_GleichesPasswortZweiMal_ErgibtVerschiedeneSaltsUndHashes()
    {
        // Arrange
        var hasher = new PasswortHasher(SchnelleIterationen);
        var passwort = TestDaten.ZufaelligesPasswort();

        // Act
        var erster = hasher.Hashe(passwort);
        var zweiter = hasher.Hashe(passwort);

        // Assert
        Assert.NotEqual(erster.Salt, zweiter.Salt);
        Assert.NotEqual(erster.Hash, zweiter.Hash);
    }

    [Fact]
    public void Pruefe_RichtigesPasswort_GibtTrueZurueck()
    {
        // Arrange
        var hasher = new PasswortHasher(SchnelleIterationen);
        var passwort = TestDaten.ZufaelligesPasswort();
        var (hash, salt) = hasher.Hashe(passwort);

        // Act
        var istKorrekt = hasher.Pruefe(passwort, hash, salt, SchnelleIterationen);

        // Assert
        Assert.True(istKorrekt, "Das richtige Passwort muss akzeptiert werden.");
    }

    [Fact]
    public void Pruefe_FalschesPasswort_GibtFalseZurueck()
    {
        // Arrange
        var hasher = new PasswortHasher(SchnelleIterationen);
        var (hash, salt) = hasher.Hashe(TestDaten.ZufaelligesPasswort());

        // Act
        var istKorrekt = hasher.Pruefe(TestDaten.ZufaelligesPasswort(), hash, salt, SchnelleIterationen);

        // Assert
        Assert.False(istKorrekt, "Ein falsches Passwort darf nicht akzeptiert werden.");
    }

    [Fact]
    public void Pruefe_SaltEinesAnderenHashes_GibtFalseZurueck()
    {
        // Arrange
        var hasher = new PasswortHasher(SchnelleIterationen);
        var passwort = TestDaten.ZufaelligesPasswort();
        var (hash, _) = hasher.Hashe(passwort);
        var (_, fremderSalt) = hasher.Hashe(passwort);

        // Act
        var istKorrekt = hasher.Pruefe(passwort, hash, fremderSalt, SchnelleIterationen);

        // Assert
        Assert.False(istKorrekt, "Der Hash hängt vom Salt ab, ein fremder Salt darf nicht passen.");
    }

    [Fact]
    public void Pruefe_HashMitAlterIterationszahl_FunktioniertAuchWennDerHasherSpaeterMehrIterationenNutzt()
    {
        // Arrange
        var altHasher = new PasswortHasher(iterationen: 1000);
        var neuHasher = new PasswortHasher(iterationen: 2000);
        var passwort = TestDaten.ZufaelligesPasswort();
        var (hash, salt) = altHasher.Hashe(passwort);

        // Act
        var istKorrekt = neuHasher.Pruefe(passwort, hash, salt, iterationen: 1000);

        // Assert
        Assert.True(istKorrekt, "Ein mit alter Iterationszahl gespeicherter Hash muss weiter gelten.");
    }

    [Fact]
    public void StandardIterationen_EntsprichtDemOwaspMindestwertFuerPbkdf2Sha256()
    {
        Assert.True(PasswortHasher.StandardIterationen >= 600_000);
        Assert.Equal(PasswortHasher.StandardIterationen, new PasswortHasher().Iterationen);
    }
}
