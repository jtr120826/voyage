namespace Reiseplaner.Models;

/// <summary>
/// Die erlaubten Kategorien eines Programmpunkts.
/// </summary>
public static class Kategorien
{
    public const string Sehenswuerdigkeit = "Sehenswürdigkeit";
    public const string Transport = "Transport";
    public const string Unterkunft = "Unterkunft";
    public const string Essen = "Essen";

    public static readonly IReadOnlyList<string> Alle =
    [
        Sehenswuerdigkeit,
        Transport,
        Unterkunft,
        Essen
    ];
}
