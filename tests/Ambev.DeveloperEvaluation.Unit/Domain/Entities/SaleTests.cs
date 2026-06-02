using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    private static ExternalIdentity Customer => new(Guid.NewGuid(), "Acme Corp");
    private static ExternalIdentity Branch => new(Guid.NewGuid(), "Downtown");
    private static ExternalIdentity Product => new(Guid.NewGuid(), "Beer 600ml");

    [Fact]
    public void GivenValidData_WhenCreate_ThenCalculatesTotalsWithDiscount()
    {
        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            Customer,
            Branch,
            [(Product, 5, 10m)]);

        Assert.Equal(SaleStatus.Active, sale.Status);
        Assert.Single(sale.Items);
        Assert.Equal(10m, sale.Items.First().DiscountPercent);
        Assert.Equal(45m, sale.TotalAmount);
        Assert.Single(sale.DomainEvents);
    }

    [Fact]
    public void GivenCancelledSale_WhenUpdate_ThenThrows()
    {
        var sale = Sale.Create("SALE-002", DateTime.UtcNow, Customer, Branch, [(Product, 2, 10m)]);
        sale.ClearDomainEvents();
        sale.Cancel();

        Assert.Throws<SaleAlreadyCancelledException>(() =>
            sale.Update(DateTime.UtcNow, Customer, Branch, [(Product, 2, 10m)]));
    }

    [Fact]
    public void GivenMultiItemSale_WhenCancelItem_ThenRecalculatesTotal()
    {
        var product2 = new ExternalIdentity(Guid.NewGuid(), "Soda");
        var sale = Sale.Create(
            "SALE-003",
            DateTime.UtcNow,
            Customer,
            Branch,
            [(Product, 2, 10m), (product2, 3, 5m)]);

        sale.ClearDomainEvents();
        var itemId = sale.Items.First().Id;
        sale.CancelItem(itemId);

        Assert.Equal(15m, sale.TotalAmount);
        Assert.True(sale.Items.First().IsCancelled);
    }

    public static IEnumerable<object[]> QuantityLineTotalCases =>
    [
        [4, 40m],
        [5, 45m],
        [9, 81m],
        [10, 80m],
        [20, 160m]
    ];

    [Theory]
    [MemberData(nameof(QuantityLineTotalCases))]
    public void GivenQuantity_WhenCreate_ThenAppliesExpectedLineTotal(int quantity, decimal expectedTotal)
    {
        var sale = Sale.Create(
            $"SALE-QTY-{quantity}",
            DateTime.UtcNow,
            Customer,
            Branch,
            [(Product, quantity, 10m)]);

        Assert.Equal(expectedTotal, sale.TotalAmount);
    }

    [Fact]
    public void GivenQuantityAboveMax_WhenCreate_ThenThrows()
    {
        Assert.Throws<MaxQuantityExceededException>(() =>
            Sale.Create("SALE-OVER", DateTime.UtcNow, Customer, Branch, [(Product, 21, 10m)]));
    }

    [Fact]
    public void GivenCancelledItem_WhenCancelItemAgain_ThenThrows()
    {
        var sale = Sale.Create("SALE-004", DateTime.UtcNow, Customer, Branch, [(Product, 2, 10m)]);
        var itemId = sale.Items.First().Id;
        sale.CancelItem(itemId);

        Assert.Throws<SaleItemAlreadyCancelledException>(() => sale.CancelItem(itemId));
    }

    [Fact]
    public void GivenMissingItem_WhenCancelItem_ThenThrowsDomainException()
    {
        var sale = Sale.Create("SALE-005", DateTime.UtcNow, Customer, Branch, [(Product, 2, 10m)]);

        Assert.Throws<DomainException>(() => sale.CancelItem(Guid.NewGuid()));
    }
}
