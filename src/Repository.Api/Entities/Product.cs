using Repository.Api.Attributes;
using Repository.Api.Entities.Interfaces;

namespace Repository.Api.Entities;

public class Product : BaseEntity, ISoftDelete
{
    [Sortable] // abilitata per il sorting
    public string Name { get; set; } = null!;

    [Sortable] // abilitata per il sorting
    public decimal Price { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}