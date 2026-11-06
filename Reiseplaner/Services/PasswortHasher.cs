using System.Security.Cryptography;

namespace Reiseplaner.Services;

/// <summary>
/// Hasht und prüft Passwörter mit PBKDF2 (HMAC-SHA256) und einem zufälligen Salt pro Passwort.
/// Passwörter werden nie im Klartext gespeichert und nie ausgegeben oder geloggt.
/// </summary>
public class PasswortHasher
{
    /// <summary>
    /// Mindestanzahl Iterationen laut OWASP Password Storage Cheat Sheet für PBKDF2-HMAC-SHA256.
    /// </summary>
    public const int StandardIterationen = 600_000;

    private const int SaltLaengeInBytes = 16;
    private const int HashLaengeInBytes = 32;

    private readonly int _iterationen;

    public PasswortHasher(int iterationen = StandardIterationen)
    {
        _iterationen = iterationen;
    }

    /// <summary>
    /// Die Iterationen, mit denen neue Hashes erzeugt werden. Sie werden pro Benutzer mitgespeichert,
    /// damit sich der Wert später erhöhen lässt, ohne bestehende Hashes ungültig zu machen.
    /// </summary>
    public int Iterationen => _iterationen;

    /// <summary>
    /// Berechnet Hash und neuen zufälligen Salt (beide Base64) für das Passwort.
    /// </summary>
    public (string Hash, string Salt) Hashe(string passwort)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltLaengeInBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(passwort, salt, _iterationen, HashAlgorithmName.SHA256, HashLaengeInBytes);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    /// <summary>
    /// Prüft ein eingegebenes Passwort gegen den gespeicherten Hash. Der Vergleich dauert immer gleich lang,
    /// damit die Antwortzeit nichts über den Hash verrät (Timing-Angriff).
    /// </summary>
    public bool Pruefe(string passwort, string gespeicherterHash, string gespeicherterSalt, int iterationen)
    {
        var salt = Convert.FromBase64String(gespeicherterSalt);
        var neuerHash = Rfc2898DeriveBytes.Pbkdf2(passwort, salt, iterationen, HashAlgorithmName.SHA256, HashLaengeInBytes);

        return CryptographicOperations.FixedTimeEquals(neuerHash, Convert.FromBase64String(gespeicherterHash));
    }
}
