namespace Reiseplaner.Dtos;

public class ReiseDto
{
    public int Id { get; set; }

    public string Titel { get; set; } = "";

    public string Zielort { get; set; } = "";

    public string Startdatum { get; set; } = "";

    public string Enddatum { get; set; } = "";

    public decimal Budget { get; set; }

    public decimal GeplantesBudget { get; set; }

    public decimal VerbleibendesBudget { get; set; }

    public bool IstBudgetUeberschritten { get; set; }
}
