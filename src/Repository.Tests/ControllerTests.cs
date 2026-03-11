using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository.Api.Controllers;
using Repository.Api.Data;
using Repository.Api.Entities;
using Repository.Api.Mapping;
using Repository.Api.Providers;
using Repository.Api.Providers.Interfaces;
using Repository.Api.Repositories;
using Repository.Api.Repositories.Interfaces;

namespace Repository.Tests;

public class ControllerTests
{
    private ServiceProvider BuildServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var map = new Dictionary<Type, string[]>
        {
            [typeof(Product)] = ["Id", "Name", "Price"]
        };
        services.AddSingleton<ISortablePropertiesProvider>(new SortablePropertiesProvider(map));

        services.AddAutoMapper(typeof(MappingProfile));
        services.AddScoped<ProductsController>();

        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task GetAll_WithInvalidSortBy_ReturnsBadRequest()
    {
        var sp = BuildServiceProvider($"db_{Guid.NewGuid()}");
        // seed some data
        using (var ctx = sp.GetRequiredService<AppDbContext>())
        {
            ctx.Products.Add(new Product { Name = "a", Price = 1 });
            ctx.Products.Add(new Product { Name = "b", Price = 2 });
            await ctx.SaveChangesAsync();
        }

        var controller = sp.GetRequiredService<ProductsController>();

        var qp = new QueryParameters { Page = 1, PageSize = 10, SortBy = "BadProp", Desc = false };
        var actionResult = await controller.GetAll(qp);

        Assert.IsType<BadRequestObjectResult>(actionResult);
    }
}
