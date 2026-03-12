using Repository.Api.Providers.Interfaces;

namespace Repository.Api.Providers;

public class SortablePropertiesProvider : ISortablePropertiesProvider
{
    // mappa Type -> proprietà ordinabili
    private readonly Dictionary<Type, string[]> map = [];

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
        }
    }

    public IEnumerable<string> GetSortableProperties(Type entityType)
    {
        if (entityType == null)
        {
            throw new ArgumentNullException(nameof(entityType));
        }

        if (map.TryGetValue(entityType, out var props))
        {
            return props;
        }

        return Array.Empty<string>();
    }

    public IEnumerable<string> GetSortableProperties<T>() => GetSortableProperties(typeof(T));

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

        // cerca in tutti gli assembly caricati (prima preferiamo l'assembly entry / esecutivo)
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Prima tentiamo assembly entry / calling assembly per velocizzare
        var preferredOrder = assemblies.OrderBy(a => a.FullName?.Contains("Microsoft") == true ? 1 : 0); // leggera preferenza

        foreach (var asm in preferredOrder)
        {
            try
            {
                var found = asm.GetTypes().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(x.FullName, name, StringComparison.OrdinalIgnoreCase));

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