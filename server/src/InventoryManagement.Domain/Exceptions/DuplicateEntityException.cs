namespace InventoryManagement.Domain.Exceptions;

public sealed class DuplicateEntityException : DomainException
{
    public string EntityName { get; }
    public string EntityKey { get; }

    public DuplicateEntityException(string entityName, string entityKey)
        : base($"{entityName} with key '{entityKey}' already exists.")
    {
        EntityName = entityName;
        EntityKey = entityKey;
    }
}
