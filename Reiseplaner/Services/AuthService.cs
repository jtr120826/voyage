using Reiseplaner.DataAccess;
using Reiseplaner.Dtos;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

/// <summary>
/// Authentifizierung (Wer bist du?): Konten anlegen und anmelden.
/// Die Berechtigungen (Was darfst du?) prüft die <see cref="IBenutzerSitzung"/> in den anderen Services.
/// </summary>
public class AuthService
{
    public const int MindestlaengePasswort = 10;

    private readonly IBenutzerRepository _repository;
    private readonly PasswortHasher _hasher;
    private readonly IBenutzerSitzung _sitzung;

    public AuthService(IBenutzerRepository repository, PasswortHasher hasher, IBenutzerSitzung sitzung)
    {
        _repository = repository;
        _hasher = hasher;
        _sitzung = sitzung;
    }

    /// <summary>
    /// Gibt an, ob schon mindestens ein Benutzerkonto existiert. Ohne Konto startet die App mit der Ersteinrichtung.
    /// </summary>
    public bool GibtEsBenutzer() => _repository.GetAll().Count > 0;

    /// <summary>
    /// Legt das erste Konto an, es wird Administrator. Geht nur, solange es noch keinen Benutzer gibt,
    /// danach legen Administratoren weitere Konten an. So gibt es kein fest eingebautes Standardpasswort.
    /// </summary>
    public void ErstelleErstenAdministrator(string benutzername, string passwort)
    {
        if (GibtEsBenutzer())
        {
            throw new InvalidOperationException("Die Ersteinrichtung ist bereits abgeschlossen.");
        }

        Speichere(benutzername, passwort, Rolle.Administrator);
    }

    /// <summary>
    /// Legt ein weiteres Konto an. Nur Administratoren dürfen das.
    /// </summary>
    public void ErstelleBenutzer(string benutzername, string passwort, Rolle rolle)
    {
        _sitzung.Pruefe(Recht.BenutzerVerwalten);

        Speichere(benutzername, passwort, rolle);
    }

    /// <summary>
    /// Meldet den Benutzer an und setzt ihn in der Sitzung.
    /// </summary>
    /// <returns>Der angemeldete Benutzer oder null. Bei unbekanntem Namen und falschem Passwort ist die Antwort gleich,
    /// damit niemand herausfinden kann, welche Benutzernamen es gibt.</returns>
    public BenutzerDto? MeldeAn(string benutzername, string passwort)
    {
        var benutzer = _repository.GetByBenutzername(Normalisiere(benutzername));
        if (benutzer == null || string.IsNullOrEmpty(passwort))
        {
            return null;
        }

        if (!_hasher.Pruefe(passwort, benutzer.PasswortHash, benutzer.Salt, benutzer.Iterationen))
        {
            return null;
        }

        var dto = new BenutzerDto { Id = benutzer.Id, Benutzername = benutzer.Benutzername, Rolle = benutzer.Rolle };
        _sitzung.Anmelden(dto);
        return dto;
    }

    /// <summary>
    /// Meldet den aktuellen Benutzer ab.
    /// </summary>
    public void MeldeAb() => _sitzung.Abmelden();

    private void Speichere(string benutzername, string passwort, Rolle rolle)
    {
        var name = Normalisiere(benutzername);
        if (name.Length == 0)
        {
            throw new ArgumentException("Bitte einen Benutzernamen eingeben.");
        }

        if (string.IsNullOrEmpty(passwort) || passwort.Length < MindestlaengePasswort)
        {
            throw new ArgumentException($"Das Passwort muss mindestens {MindestlaengePasswort} Zeichen lang sein.");
        }

        if (_repository.GetByBenutzername(name) != null)
        {
            throw new ArgumentException("Dieser Benutzername ist bereits vergeben.");
        }

        var (hash, salt) = _hasher.Hashe(passwort);
        _repository.Add(new Benutzer
        {
            Benutzername = name,
            PasswortHash = hash,
            Salt = salt,
            Iterationen = _hasher.Iterationen,
            Rolle = rolle
        });
    }

    private static string Normalisiere(string? benutzername) => (benutzername ?? "").Trim().ToLowerInvariant();
}
