using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Repository.Api.Entities;
using Repository.Api.Providers.Interfaces;
using Repository.Api.Repositories.Interfaces;

namespace Repository.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(IUnitOfWork uow, IMapper mapper, ISortablePropertiesProvider sortableProvider) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryParameters qp)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var repo = uow.Repository<Product>();
            var paged = await repo.GetPagedAsync(qp);
            var dtoItems = paged.Items.Select(p => mapper.Map<ProductDto>(p));
            var result = new PagedResult<ProductDto>(dtoItems, paged.TotalCount, paged.Page, paged.PageSize);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("sortables")]
    public IActionResult GetSortables()
    {
        var list = sortableProvider.GetSortableProperties<Product>();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<ProductDto>(product));
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        var repo = uow.Repository<Product>();
        var entity = mapper.Map<Product>(dto);
        await repo.AddAsync(entity);
        await uow.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, mapper.Map<ProductDto>(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        mapper.Map(dto, product);
        repo.Update(product);
        await uow.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id);

        if (product is null)
        {
            return NotFound();
        }

        await repo.DeleteAsync(product);
        await uow.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("hard/{id:int}")]
    public async Task<IActionResult> HardDelete(int id)
    {
        var repo = uow.Repository<Product>();
        var product = await repo.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        await repo.HardDeleteAsync(product);
        await uow.SaveChangesAsync();

        return NoContent();
    }
}