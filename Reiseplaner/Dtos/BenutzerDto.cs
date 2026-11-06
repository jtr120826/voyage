using Reiseplaner.Models;

namespace Reiseplaner.Dtos;

/// <summary>
/// Angemeldeter Benutzer in der Form, wie die Oberfläche ihn kennt. Passwort-Hash, Salt und Iterationen
/// bleiben bewusst draussen, sie verlassen die Datenzugriffsschicht nie.
/// </summary>
public class BenutzerDto
{
    public int Id { get; set; }

    public string Benutzername { get; set; } = "";

    public Rolle Rolle { get; set; }
}
