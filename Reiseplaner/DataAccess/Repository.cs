using Microsoft.EntityFrameworkCore;

namespace Reiseplaner.DataAccess;

/// <summary>
/// Standardimplementierung von <see cref="IRepository{T}"/> mit EF Core.
/// Unterklassen überschreiben nur, was von diesem Standard abweicht.
/// </summary>
/// <typeparam name="T">Die Entity-Klasse.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    /// <inheritdoc />
    public virtual List<T> GetAll()
    {
        using var context = new AppDbContext();
        return context.Set<T>().ToList();
    }

    /// <inheritdoc />
    public virtual T? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Set<T>().Find(id);
    }

    /// <inheritdoc />
    public virtual void Add(T entity)
    {
        using var context = new AppDbContext();
        context.Set<T>().Add(entity);
        context.SaveChanges();
    }

    /// <inheritdoc />
    public virtual void Update(T entity)
    {
        using var context = new AppDbContext();
        context.Set<T>().Update(entity);
        context.SaveChanges();
    }

    /// <inheritdoc />
    public virtual void Delete(int id)
    {
        using var context = new AppDbContext();
        var entity = context.Set<T>().Find(id);
        if (entity == null) return;

        context.Set<T>().Remove(entity);
        context.SaveChanges();
    }
}
