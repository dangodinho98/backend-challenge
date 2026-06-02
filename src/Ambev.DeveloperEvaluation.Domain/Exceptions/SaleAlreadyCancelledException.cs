namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

public class SaleAlreadyCancelledException : DomainException
{
    public SaleAlreadyCancelledException(Guid saleId)
        : base($"Sale '{saleId}' is already cancelled.")
    {
    }
}
