namespace Repository.Api.Providers.Interfaces;

public interface ISortablePropertiesProvider
{
    IEnumerable<string> GetSortableProperties(Type entityType);
    IEnumerable<string> GetSortableProperties<T>();
    /// <summary>
    /// Restituisce le proprietà ordinabili per il nome dell'entità (es. "Product" o full type name).
    /// Ritorna una sequenza vuota se l'entità non è configurata / non risolvibile.
    /// </summary>
    IEnumerable<string> GetSortablePropertiesByName(string entityName);

    /// <summary>
    /// Restituisce i nomi delle entità (chiavi) presenti nella configurazione.
    /// </summary>
    IEnumerable<string> GetConfiguredEntityNames();
}