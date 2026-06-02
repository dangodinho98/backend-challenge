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
}
