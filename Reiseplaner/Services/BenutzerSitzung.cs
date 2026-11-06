using Reiseplaner.Dtos;
using Reiseplaner.Models;

namespace Reiseplaner.Services;

/// <inheritdoc />
public class BenutzerSitzung : IBenutzerSitzung
{
    /// <inheritdoc />
    public BenutzerDto? AngemeldeterBenutzer { get; private set; }

    /// <inheritdoc />
    public void Anmelden(BenutzerDto benutzer) => AngemeldeterBenutzer = benutzer;

    /// <inheritdoc />
    public void Abmelden() => AngemeldeterBenutzer = null;

    /// <inheritdoc />
    public bool Darf(Recht recht)
    {
        return AngemeldeterBenutzer != null
            && Berechtigungen.IstErlaubt(AngemeldeterBenutzer.Rolle, recht);
    }

    /// <inheritdoc />
    public void Pruefe(Recht recht)
    {
        if (AngemeldeterBenutzer == null)
        {
            throw new UnauthorizedAccessException("Bitte zuerst anmelden.");
        }

        if (!Berechtigungen.IstErlaubt(AngemeldeterBenutzer.Rolle, recht))
        {
            throw new UnauthorizedAccessException(
                $"Die Rolle {AngemeldeterBenutzer.Rolle} hat dafür keine Berechtigung ({recht}).");
        }
    }
}
