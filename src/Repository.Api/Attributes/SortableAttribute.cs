namespace Repository.Api.Attributes;

/// <summary>
/// Segnala che una proprietà è utilizzabile per l'ordinamento dinamico (sortBy).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class SortableAttribute : Attribute
{ }