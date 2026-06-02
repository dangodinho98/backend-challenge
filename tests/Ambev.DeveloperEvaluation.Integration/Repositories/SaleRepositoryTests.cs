using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class SaleRepositoryTests : IDisposable
{
    private readonly DefaultContext _context;
    private readonly ISaleRepository _repository;

    public SaleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseInMemoryDatabase($"SaleRepositoryTests_{Guid.NewGuid()}")
            .Options;

        _context = new DefaultContext(options);
        _repository = new SaleRepository(_context);
    }

    [Fact(DisplayName = "Given sale When creating and fetching Then returns persisted sale with items")]
    public async Task CreateAndGetById_ReturnsPersistedSale()
    {
        var sale = BuildSale("SALE-INT-001");

        await _repository.CreateAsync(sale);
        var fetched = await _repository.GetByIdAsync(sale.Id);

        fetched.Should().NotBeNull();
        fetched!.SaleNumber.Should().Be("SALE-INT-001");
        fetched.Items.Should().HaveCount(1);
        fetched.TotalAmount.Should().Be(45m);
    }

    [Fact(DisplayName = "Given sales When listing with filter Then returns matching page")]
    public async Task ListAsync_WithSaleNumberFilter_ReturnsMatchingSales()
    {
        await _repository.CreateAsync(BuildSale("SALE-ALPHA-001"));
        await _repository.CreateAsync(BuildSale("SALE-BETA-002"));

        var (items, totalCount) = await _repository.ListAsync(new SaleListCriteria
        {
            Page = 1,
            Size = 10,
            SaleNumber = "SALE-ALPHA*"
        });

        totalCount.Should().Be(1);
        items.Should().ContainSingle(s => s.SaleNumber == "SALE-ALPHA-001");
    }

    [Fact(DisplayName = "Given sale When deleting Then removes sale from database")]
    public async Task DeleteAsync_ExistingSale_RemovesSale()
    {
        var sale = BuildSale("SALE-INT-DEL");
        await _repository.CreateAsync(sale);

        var deleted = await _repository.DeleteAsync(sale.Id);
        var fetched = await _repository.GetByIdAsync(sale.Id);

        deleted.Should().BeTrue();
        fetched.Should().BeNull();
    }

    [Fact(DisplayName = "Given sale When updating Then persists changes")]
    public async Task UpdateAsync_ExistingSale_PersistsChanges()
    {
        var sale = BuildSale("SALE-INT-UPD");
        await _repository.CreateAsync(sale);

        var tracked = await _repository.GetByIdForUpdateAsync(sale.Id);
        tracked!.Update(
            DateTime.UtcNow.AddDays(1),
            new ExternalIdentity(Guid.NewGuid(), "New Customer"),
            new ExternalIdentity(Guid.NewGuid(), "New Branch"),
            [(new ExternalIdentity(Guid.NewGuid(), "Soda"), 6, 5m)]);

        await _repository.UpdateAsync(tracked);

        var fetched = await _repository.GetByIdAsync(sale.Id);
        fetched!.Customer.Description.Should().Be("New Customer");
        fetched.Items.Should().ContainSingle();
        fetched.TotalAmount.Should().Be(27m);
    }

    [Fact(DisplayName = "Given missing sale When deleting Then returns false")]
    public async Task DeleteAsync_MissingSale_ReturnsFalse()
    {
        var deleted = await _repository.DeleteAsync(Guid.NewGuid());

        deleted.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private static Sale BuildSale(string saleNumber) =>
        Sale.Create(
            saleNumber,
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 5, 10m)]);
}
