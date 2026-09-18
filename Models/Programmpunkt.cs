using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reiseplaner.Models;

public class Programmpunkt
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Titel { get; set; } = "";

    [Required]
    public string Datum { get; set; } = "";

    [Required]
    public string Kategorie { get; set; } = "";

    public decimal Kosten { get; set; }

    public bool Erledigt { get; set; }

    // Fremdschlüssel zur zugehörigen Reise (n:1-Beziehung)
    [ForeignKey(nameof(Reise))]
    public int ReiseId { get; set; }

    public Reise? Reise { get; set; }
}
