using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Repository.Api.Entities;
using Repository.Api.Entities.Interfaces;

namespace Repository.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applica global query filter a tutte le entità che implementano ISoftDelete
        var softDeleteInterface = typeof(ISoftDelete);
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(t => softDeleteInterface.IsAssignableFrom(t.ClrType))
            .Select(t => t.ClrType);

        foreach (var type in entityTypes)
        {
            // costruisci lambda: (e) => EF.Property<bool>(e, "IsDeleted") == false
            var parameter = Expression.Parameter(type, "e");

            var prop = Expression.PropertyOrField(parameter, nameof(ISoftDelete.IsDeleted));
            var body = Expression.Equal(prop, Expression.Constant(false));

            var delegateType = typeof(Func<,>).MakeGenericType(type, typeof(bool));
            var lambda = Expression.Lambda(delegateType, body, parameter);
            modelBuilder.Entity(type).HasQueryFilter(lambda);
        }

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplySoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplySoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted))
        {
            if (entry.Entity is ISoftDelete sd)
            {
                sd.IsDeleted = true;
                sd.DeletedAt = DateTime.UtcNow;
                entry.State = EntityState.Modified;
            }
        }
    }
}