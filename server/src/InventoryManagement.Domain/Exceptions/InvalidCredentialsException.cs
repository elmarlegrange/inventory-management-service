namespace InventoryManagement.Domain.Exceptions;

public sealed class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException(string message = "Invalid username or password.")
        : base(message)
    {
    }
}
