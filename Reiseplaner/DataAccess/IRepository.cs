namespace Reiseplaner.DataAccess;

/// <summary>
/// Gemeinsamer Vertrag für den Datenzugriff auf eine Entity.
/// </summary>
/// <typeparam name="T">Die Entity-Klasse.</typeparam>
public interface IRepository<T>
{
    /// <summary>
    /// Lädt alle Einträge.
    /// </summary>
    List<T> GetAll();

    /// <summary>
    /// Lädt einen Eintrag anhand seiner Id.
    /// </summary>
    /// <returns>Der Eintrag oder null, wenn er nicht existiert.</returns>
    T? GetById(int id);

    /// <summary>
    /// Speichert einen neuen Eintrag.
    /// </summary>
    void Add(T entity);

    /// <summary>
    /// Speichert die Änderungen an einem bestehenden Eintrag.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Löscht den Eintrag mit der angegebenen Id. Existiert er nicht, passiert nichts.
    /// </summary>
    void Delete(int id);
}
