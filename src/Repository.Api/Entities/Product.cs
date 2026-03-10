using Repository.Api.Entities.Interfaces;

namespace Repository.Api.Entities;

public class Product : BaseEntity, ISoftDelete
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}