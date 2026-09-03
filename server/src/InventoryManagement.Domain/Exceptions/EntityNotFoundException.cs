namespace InventoryManagement.Domain.Exceptions;

public sealed class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public string EntityKey { get; }

    public EntityNotFoundException(string entityName, string entityKey)
        : base($"{entityName} with key '{entityKey}' was not found.")
    {
        EntityName = entityName;
        EntityKey = entityKey;
    }
}
