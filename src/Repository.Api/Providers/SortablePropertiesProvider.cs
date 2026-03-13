using Repository.Api.Providers.Interfaces;

namespace Repository.Api.Providers;

public class SortablePropertiesProvider : ISortablePropertiesProvider
{
    // mappa Type -> proprietà ordinabili
    private readonly Dictionary<Type, string[]> map = [];

    // mappa dei nomi configurati (case-preserving) -> Type risolto
    private readonly Dictionary<string, Type> nameToType = new(StringComparer.OrdinalIgnoreCase);

    // costruttore riceve la mappa da configuration (chiave: entityName, valore: lista proprietà)
    // entityName può essere il nome semplice della classe ("Product") oppure il full type name.
    public SortablePropertiesProvider(IDictionary<string, string[]> configurationMap)
    {
        ArgumentNullException.ThrowIfNull(configurationMap);

        foreach (var kv in configurationMap)
        {
            var key = kv.Key;
            var props = kv.Value ?? Array.Empty<string>();

            var resolved = ResolveTypeByName(key);
            if (resolved == null)
            {
                // non falliamo, ma segnaliamo: puoi cambiare qui per lanciare eccezione se preferisci
                Console.WriteLine($"[SortablePropertiesProvider] Warning: unable to resolve entity type for key '{key}'. Entry will be ignored.");
                continue;
            }

            map[resolved] = props;
            // mantiene la chiave originale (ma la ricerca è case-insensitive grazie al comparer)
            nameToType[key] = resolved;
        }
    }

    public IEnumerable<string> GetSortableProperties(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        if (map.TryGetValue(entityType, out var props))
        {
            return props;
        }

        return [];
    }

    public IEnumerable<string> GetSortableProperties<T>() => GetSortableProperties(typeof(T));

    public IEnumerable<string> GetSortablePropertiesByName(string entityName)
    {
        if (string.IsNullOrWhiteSpace(entityName))
        {
            return [];
        }

        // tenta lookup diretto tra le chiavi configurate (case-insensitive)
        if (nameToType.TryGetValue(entityName, out var t))
        {
            return GetSortableProperties(t);
        }

        // come fallback, prova a risolvere il type dinamicamente (supporta full type name e altri assembly)
        var resolved = ResolveTypeByName(entityName);

        if (resolved == null)
        {
            return [];
        }

        return GetSortableProperties(resolved);
    }

    public IEnumerable<string> GetConfiguredEntityNames() => nameToType.Keys;

    private static Type? ResolveTypeByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        // prova a risolvere come full type name
        var t = Type.GetType(name);
        if (t != null)
        {
            return t;
        }

        // cerca in tutti gli assembly caricati
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Prima tentiamo assembly entry / calling assembly per velocizzare
        var preferredOrder = assemblies.OrderBy(a
            => a.FullName?.StartsWith("Microsoft", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0);

        foreach (var asm in preferredOrder)
        {
            try
            {
                var found = asm.GetTypes().FirstOrDefault(x =>
                    string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.FullName, name, StringComparison.OrdinalIgnoreCase));

                if (found != null)
                {
                    return found;
                }
            }
            catch
            {
                // ignoriamo assembly che non possono essere ispezionati
            }
        }

        return null;
    }
}