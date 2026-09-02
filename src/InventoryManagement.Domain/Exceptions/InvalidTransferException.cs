namespace InventoryManagement.Domain.Exceptions;

public sealed class InvalidTransferException : DomainException
{
    public InvalidTransferException(string message) : base(message) { }
}
