using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Services;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; private set; }
    public ExternalIdentity Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public decimal LineTotal { get; private set; }
    public bool IsCancelled { get; private set; }

    private SaleItem()
    {
    }

    internal void AssignToSale(Guid saleId) => SaleId = saleId;

    internal SaleItem(ExternalIdentity product, int quantity, decimal unitPrice)
    {
        SetProduct(product);
        UpdatePricing(quantity, unitPrice);
    }

    internal void Cancel()
    {
        if (IsCancelled)
            throw new SaleItemAlreadyCancelledException(Id);

        IsCancelled = true;
        LineTotal = 0;
        DiscountPercent = 0;
    }

    private void SetProduct(ExternalIdentity product)
    {
        Product = product ?? throw new DomainException("Product reference is required.");
    }

    private void UpdatePricing(int quantity, decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        Quantity = quantity;
        UnitPrice = unitPrice;
        DiscountPercent = DiscountPolicy.GetDiscountPercent(quantity);
        LineTotal = DiscountPolicy.CalculateLineTotal(quantity, unitPrice);
    }
}
