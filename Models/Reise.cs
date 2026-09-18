using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    // 1:n-Navigation Property zu den Programmpunkten dieser Reise
    public ICollection<Programmpunkt> Programmpunkte { get; set; } = new List<Programmpunkt>();

    // Nicht in der Datenbank gespeichert - wird nach dem Laden aus den Programmpunkten berechnet
    [NotMapped]
    public decimal GeplantesBudget { get; set; }

    [NotMapped]
    public decimal VerbleibendesBudget => Budget - GeplantesBudget;
}
