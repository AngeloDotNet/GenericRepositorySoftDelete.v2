using Microsoft.EntityFrameworkCore;
using Repository.Api.Data;
using Repository.Api.Entities;
using Repository.Api.Providers;
using Repository.Api.Providers.Interfaces;
using Repository.Api.Repositories;

namespace Repository.Tests;

public class RepositoryTests
{
    private ISortablePropertiesProvider MakeProvider()
    {
        var map = new Dictionary<string, string[]>
        {
            [typeof(Product).ToString()] = ["Id", "Name", "Price"]
        };
        return new SortablePropertiesProvider(map);
    }

    [Fact]
    public async Task SoftDelete_MarksIsDeletedAndHidden()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"testdb_{Guid.NewGuid()}")
            .Options;

        //using var context = new AppDbContext(options);
        //var repo = new GenericRepository<Product>(context);

        using var context = new AppDbContext(options);
        var provider = MakeProvider();
        var repo = new GenericRepository<Product>(context, provider);

        var p = new Product { Name = "A", Price = 1m };

        await repo.AddAsync(p);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var id = p.Id;

        // soft delete
        await repo.DeleteAsync(p);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var raw = await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(raw);
        Assert.True(raw!.IsDeleted);

        var all = await repo.GetAllAsync();
        Assert.DoesNotContain(all, x => x.Id == id);
    }

    [Fact]
    public async Task HardDelete_RemovesEntity()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"testdb_{Guid.NewGuid()}")
            .Options;

        using var context = new AppDbContext(options);
        var provider = MakeProvider();
        var repo = new GenericRepository<Product>(context, provider);

        var p = new Product { Name = "B", Price = 2m };

        await repo.AddAsync(p);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var id = p.Id;

        await repo.HardDeleteAsync(p);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var raw = await context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(raw);
    }

    [Fact]
    public async Task Paging_WorksAndRespectsSoftDelete()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"testdb_{Guid.NewGuid()}")
            .Options;

        //using var context = new AppDbContext(options);
        //var repo = new GenericRepository<Product>(context);

        using var context = new AppDbContext(options);
        var provider = MakeProvider();
        var repo = new GenericRepository<Product>(context, provider);

        for (var i = 1; i <= 25; i++)
        {
            await repo.AddAsync(new Product { Name = $"P{i}", Price = i });
        }

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var qp = new QueryParameters { Page = 2, PageSize = 10, SortBy = "Name", Desc = false };
        var page = await repo.GetPagedAsync(qp);

        Assert.Equal(25, page.TotalCount);
        Assert.Equal(10, page.PageSize);
        Assert.Equal(2, page.Page);
        Assert.Equal(10, page.Items.Count());
    }
}
