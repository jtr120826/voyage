namespace Reiseplaner.Models;

public class Programmpunkt
{
    public int Id { get; set; }
    public string Titel { get; set; } = "";
    public string Datum { get; set; } = "";
    public string Kategorie { get; set; } = "";
    public decimal Kosten { get; set; }
    public bool Erledigt { get; set; }
    public int ReiseId { get; set; }
}
