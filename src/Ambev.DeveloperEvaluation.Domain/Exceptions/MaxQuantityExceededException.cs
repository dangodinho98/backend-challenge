namespace Ambev.DeveloperEvaluation.Domain.Exceptions;

public class MaxQuantityExceededException : DomainException
{
    public MaxQuantityExceededException(int quantity)
        : base($"Cannot sell more than {Services.DiscountPolicy.MaxQuantityPerProduct} identical items. Requested quantity: {quantity}.")
    {
    }
}
