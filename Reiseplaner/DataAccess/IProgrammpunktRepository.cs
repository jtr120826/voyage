using Reiseplaner.Models;

namespace Reiseplaner.DataAccess;

/// <summary>
/// Datenzugriff auf Programmpunkte, erweitert um die Abfrage pro Reise.
/// </summary>
public interface IProgrammpunktRepository : IRepository<Programmpunkt>
{
    /// <summary>
    /// Lädt alle Programmpunkte einer Reise, sortiert nach Datum.
    /// </summary>
    List<Programmpunkt> GetByReise(int reiseId);
}
