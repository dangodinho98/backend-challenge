using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Builders;

public class SaleTestDataBuilder
{
    private readonly Faker _faker = new();

    public ExternalIdentity Customer() =>
        new(Guid.NewGuid(), _faker.Company.CompanyName());

    public ExternalIdentity Branch() =>
        new(Guid.NewGuid(), _faker.Address.City());

    public ExternalIdentity Product() =>
        new(Guid.NewGuid(), _faker.Commerce.ProductName());

    public Sale BuildSale(int quantity = 5, decimal unitPrice = 10m, int itemCount = 1)
    {
        var items = Enumerable.Range(0, itemCount)
            .Select(_ => (Product(), quantity, unitPrice))
            .ToArray();

        return Sale.Create(
            $"SALE-{_faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            _faker.Date.RecentOffset(30).UtcDateTime,
            Customer(),
            Branch(),
            items);
    }

    public CreateSaleCommand BuildCreateCommand(string? saleNumber = null, int quantity = 5, decimal unitPrice = 10m)
    {
        var customer = Customer();
        var branch = Branch();
        var product = Product();

        return new CreateSaleCommand
        {
            SaleNumber = saleNumber ?? $"SALE-{_faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            SaleDate = _faker.Date.RecentOffset(30).UtcDateTime,
            CustomerId = customer.Id,
            CustomerName = customer.Description,
            BranchId = branch.Id,
            BranchName = branch.Description,
            Items =
            [
                new CreateSaleItemCommand
                {
                    ProductId = product.Id,
                    ProductName = product.Description,
                    Quantity = quantity,
                    UnitPrice = unitPrice
                }
            ]
        };
    }
}
