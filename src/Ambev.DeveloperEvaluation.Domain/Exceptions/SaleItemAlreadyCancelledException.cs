namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

public class SaleItemAlreadyCancelledException : DomainException
{
    public SaleItemAlreadyCancelledException(Guid itemId)
        : base($"Sale item '{itemId}' is already cancelled.")
    {
    }
}
