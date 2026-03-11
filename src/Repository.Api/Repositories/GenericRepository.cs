using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Repository.Api.Data;
using Repository.Api.Entities;
using Repository.Api.Providers.Interfaces;
using Repository.Api.Repositories.Interfaces;

namespace Repository.Api.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;
    private readonly ISortablePropertiesProvider sortableProvider;

    public GenericRepository(AppDbContext context, ISortablePropertiesProvider sortableProvider)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        this.sortableProvider = sortableProvider ?? throw new ArgumentNullException(nameof(sortableProvider));
        _dbSet = _context.Set<T>();
    }

    public virtual IQueryable<T> Query() => _dbSet.AsQueryable();

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public virtual void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public virtual Task DeleteAsync(T entity)
    {
        _context.Entry(entity).State = EntityState.Deleted;
        return Task.CompletedTask;
    }

    public virtual async Task HardDeleteAsync(T entity)
    {
        _dbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public virtual async Task<PagedResult<T>> GetPagedAsync(QueryParameters parameters, Expression<Func<T, bool>>? filter = null)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        var query = _dbSet.AsQueryable();
        if (filter != null)
        {
            query = query.Where(filter);
        }

        var total = await query.CountAsync();

        // Validazione sortBy usando provider centralizzato
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            var allowed = sortableProvider.GetSortableProperties(typeof(T))
                .Select(s => s.ToLowerInvariant()).ToHashSet();

            if (!allowed.Contains(parameters.SortBy.ToLowerInvariant()))
            {
                var allowedList = sortableProvider.GetSortableProperties(typeof(T));
                throw new ArgumentException($"Invalid sortBy '{parameters.SortBy}'. Allowed properties: {string.Join(", ", allowedList)}");
            }

            // usa EF.Property con il nome esatto tra quelli consentiti (case-sensitive sul nome reale)
            // trova il nome corretto (case-preserving)
            var exact = sortableProvider.GetSortableProperties(typeof(T))
                .FirstOrDefault(s => string.Equals(s, parameters.SortBy, StringComparison.OrdinalIgnoreCase))!;

            query = parameters.Desc
                ? query.OrderByDescending(e => EF.Property<object>(e, exact))
                : query.OrderBy(e => EF.Property<object>(e, exact));
        }
        else
        {
            query = query.OrderBy(e => e.Id);
        }

        var skip = (parameters.Page - 1) * parameters.PageSize;
        if (skip < 0)
        {
            skip = 0;
        }

        var items = await query
            .Skip(skip)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResult<T>(items, total, parameters.Page, parameters.PageSize);
    }
}