using System.Collections.Concurrent;
using Repository.Api.Data;
using Repository.Api.Entities;
using Repository.Api.Repositories.Interfaces;

namespace Repository.Api.Repositories;

public class UnitOfWork(AppDbContext context, IServiceProvider serviceProvider) : IUnitOfWork, IDisposable
{
    private readonly ConcurrentDictionary<Type, object> repositories = new();

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var type = typeof(T);
        if (!repositories.TryGetValue(type, out var repoObj))
        {
            // risolvi il repository generico da DI (in modo che il DI si occupi di fornire le dipendenze)
            var repo = (IGenericRepository<T>)serviceProvider.GetRequiredService(typeof(IGenericRepository<T>));
            repositories[type] = repo;
            repoObj = repo;
        }

        return (IGenericRepository<T>)repoObj!;
    }

    public Task<int> SaveChangesAsync()
    {
        return context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}