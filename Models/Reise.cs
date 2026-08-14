namespace Reiseplaner.Models;

public class Reise
{
    public int Id { get; set; }
    public string Titel { get; set; } = "";
    public string Zielort { get; set; } = "";
    public string Startdatum { get; set; } = "";
    public string Enddatum { get; set; } = "";
    public decimal Budget { get; set; }
}
