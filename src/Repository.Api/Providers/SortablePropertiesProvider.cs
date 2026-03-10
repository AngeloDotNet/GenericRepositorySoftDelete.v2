using Repository.Api.Providers.Interfaces;

namespace Repository.Api.Providers;

public class SortablePropertiesProvider(IDictionary<Type, string[]> map) : ISortablePropertiesProvider
{
    private readonly IReadOnlyDictionary<Type, string[]> map = new Dictionary<Type, string[]>(map ?? throw new ArgumentNullException(nameof(map)));

    public IEnumerable<string> GetSortableProperties(Type entityType)
    {
        if (entityType == null)
        {
            throw new ArgumentNullException(nameof(entityType));
        }

        if (map.TryGetValue(entityType, out var arr))
        {
            return arr;
        }

        return [];
    }

    public IEnumerable<string> GetSortableProperties<T>() => GetSortableProperties(typeof(T));
}