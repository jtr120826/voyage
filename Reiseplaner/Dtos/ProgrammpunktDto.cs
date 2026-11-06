namespace Reiseplaner.Dtos;

/// <summary>
/// Programmpunkt in der Form, wie die Oberfläche ihn anzeigt (ohne Fremdschlüssel und Navigation Property).
/// </summary>
public class ProgrammpunktDto
{
    public int Id { get; set; }

    public string Titel { get; set; } = "";

    public string Datum { get; set; } = "";

    /// <summary>
    /// Einer der Werte aus <see cref="Models.Kategorien.Alle"/>.
    /// </summary>
    public string Kategorie { get; set; } = "";

    public decimal Kosten { get; set; }

    public bool Erledigt { get; set; }
}
