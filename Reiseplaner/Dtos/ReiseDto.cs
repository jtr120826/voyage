namespace Reiseplaner.Dtos;

/// <summary>
/// Reise in der Form, wie die Oberfläche sie anzeigt (inklusive berechneter Budgetwerte).
/// </summary>
public class ReiseDto
{
    public int Id { get; set; }

    public string Titel { get; set; } = "";

    public string Zielort { get; set; } = "";

    public string Startdatum { get; set; } = "";

    public string Enddatum { get; set; } = "";

    public decimal Budget { get; set; }

    /// <summary>
    /// Summe der Kosten aller Programmpunkte dieser Reise.
    /// </summary>
    public decimal GeplantesBudget { get; set; }

    /// <summary>
    /// Budget minus geplantes Budget, negativ bei Überschreitung.
    /// </summary>
    public decimal VerbleibendesBudget { get; set; }

    public bool IstBudgetUeberschritten { get; set; }
}
