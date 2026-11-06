using System.ComponentModel.DataAnnotations;

namespace Reiseplaner.Models;

/// <summary>
/// Ein Benutzerkonto. Das Passwort wird nie gespeichert, nur Hash, Salt und die verwendeten Iterationen.
/// </summary>
public class Benutzer
{
    [Key]
    public int Id { get; set; }

    /// <summary>Eindeutig, immer kleingeschrieben und ohne Leerzeichen am Rand.</summary>
    [Required]
    public string Benutzername { get; set; } = "";

    [Required]
    public string PasswortHash { get; set; } = "";

    [Required]
    public string Salt { get; set; } = "";

    /// <summary>Iterationen, mit denen der Hash erzeugt wurde (für spätere Erhöhung des Werts).</summary>
    public int Iterationen { get; set; }

    public Rolle Rolle { get; set; }
}
