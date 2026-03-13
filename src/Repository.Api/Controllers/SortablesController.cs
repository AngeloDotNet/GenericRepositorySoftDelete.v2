using Microsoft.AspNetCore.Mvc;
using Repository.Api.Providers.Interfaces;

namespace Repository.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SortablesController(ISortablePropertiesProvider sortableProvider) : ControllerBase
{

    /// <summary>
    /// GET /api/sortables
    /// - senza query param: restituisce le entità configurate
    /// - con ?entity=Product: restituisce la lista delle proprietà ordinabili per l'entità (404 se non configurata)
    /// </summary>
    [HttpGet]
    public IActionResult Get([FromQuery] string? entity)
    {
        if (string.IsNullOrWhiteSpace(entity))
        {
            var configured = sortableProvider.GetConfiguredEntityNames();
            return Ok(configured);
        }

        var props = sortableProvider.GetSortablePropertiesByName(entity).ToArray();
        if (props.Length == 0)
        {
            return NotFound(new { error = $"Entity '{entity}' is not configured or not found." });
        }

        return Ok(props);
    }
}