using Reiseplaner.Dtos;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

/// <summary>
/// Merkt sich, wer gerade angemeldet ist, und prüft dessen Rechte.
/// </summary>
public interface IBenutzerSitzung
{
    /// <summary>
    /// Der angemeldete Benutzer oder null, wenn niemand angemeldet ist.
    /// </summary>
    BenutzerDto? AngemeldeterBenutzer { get; }

    void Anmelden(BenutzerDto benutzer);

    void Abmelden();

    /// <summary>
    /// Gibt zurück, ob der angemeldete Benutzer das Recht hat. Ohne Anmeldung ist die Antwort immer nein.
    /// </summary>
    bool Darf(Recht recht);

    /// <summary>
    /// Wirft eine <see cref="UnauthorizedAccessException"/>, wenn niemand angemeldet ist oder das Recht fehlt.
    /// </summary>
    void Pruefe(Recht recht);
}
