using Microsoft.EntityFrameworkCore;

namespace Reiseplaner.DataAccess;

public class Repository<T> : IRepository<T> where T : class
{
    public virtual List<T> GetAll()
    {
        using var context = new AppDbContext();
        return context.Set<T>().ToList();
    }

    public virtual T? GetById(int id)
    {
        using var context = new AppDbContext();
        return context.Set<T>().Find(id);
    }

    public virtual void Add(T entity)
    {
        using var context = new AppDbContext();
        context.Set<T>().Add(entity);
        context.SaveChanges();
    }

    public virtual void Update(T entity)
    {
        using var context = new AppDbContext();
        context.Set<T>().Update(entity);
        context.SaveChanges();
    }

    public virtual void Delete(int id)
    {
        using var context = new AppDbContext();
        var entity = context.Set<T>().Find(id);
        if (entity == null) return;

        context.Set<T>().Remove(entity);
        context.SaveChanges();
    }
}
