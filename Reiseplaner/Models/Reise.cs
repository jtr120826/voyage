using System.ComponentModel.DataAnnotations;

namespace Reiseplaner.Models;

public class Reise
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Titel { get; set; } = "";

    [Required]
    public string Zielort { get; set; } = "";

    [Required]
    public string Startdatum { get; set; } = "";

    [Required]
    public string Enddatum { get; set; } = "";

    public decimal Budget { get; set; }

    public ICollection<Programmpunkt> Programmpunkte { get; set; } = new List<Programmpunkt>();
}
