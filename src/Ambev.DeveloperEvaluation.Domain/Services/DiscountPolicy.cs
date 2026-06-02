using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Services;

public static class DiscountPolicy
{
    public const int MaxQuantityPerProduct = 20;

    public static decimal GetDiscountPercent(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (quantity > MaxQuantityPerProduct)
            throw new MaxQuantityExceededException(quantity);

        if (quantity >= 10)
            return 20m;

        if (quantity > 4)
            return 10m;

        return 0m;
    }

    public static decimal CalculateLineTotal(int quantity, decimal unitPrice)
    {
        var discountPercent = GetDiscountPercent(quantity);
        var gross = quantity * unitPrice;
        return Math.Round(gross * (1 - discountPercent / 100m), 2, MidpointRounding.AwayFromZero);
    }
}
