namespace Repository.Api.Providers.Interfaces;

public interface ISortablePropertiesProvider
{
    IEnumerable<string> GetSortableProperties(Type entityType);
    IEnumerable<string> GetSortableProperties<T>();
}