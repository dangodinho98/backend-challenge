using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.ModifySale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IMapper _mapper;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new UpdateSaleHandler(_saleRepository, _domainEventDispatcher, _mapper);
    }

    [Fact(DisplayName = "Given existing sale When updating Then returns updated sale model")]
    public async Task Handle_ExistingSale_ReturnsUpdatedModel()
    {
        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 5, 10m)]);

        sale.ClearDomainEvents();

        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleDate = DateTime.UtcNow.AddDays(1),
            CustomerId = Guid.NewGuid(),
            CustomerName = "New Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Uptown",
            Items =
            [
                new UpdateSaleItemCommand
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Soda",
                    Quantity = 10,
                    UnitPrice = 5m
                }
            ]
        };

        var expected = new SaleModel { Id = sale.Id, SaleNumber = "SALE-001", TotalAmount = 40m };

        _saleRepository.GetByIdForUpdateAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<SaleModel>(sale).Returns(expected);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        await _saleRepository.Received(1).UpdateAsync(sale, Arg.Any<CancellationToken>());
        await _domainEventDispatcher.Received(1).DispatchAsync(
            Arg.Any<IEnumerable<Ambev.DeveloperEvaluation.Domain.Events.IDomainEvent>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When updating Then throws KeyNotFoundException")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdForUpdateAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new UpdateSaleCommand { Id = saleId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given cancelled sale When updating Then throws SaleAlreadyCancelledException")]
    public async Task Handle_CancelledSale_ThrowsSaleAlreadyCancelledException()
    {
        var sale = Sale.Create(
            "SALE-002",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 2, 10m)]);

        sale.ClearDomainEvents();
        sale.Cancel();

        _saleRepository.GetByIdForUpdateAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new UpdateSaleCommand
        {
            Id = sale.Id,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch",
            Items = [new UpdateSaleItemCommand { ProductId = Guid.NewGuid(), ProductName = "Item", Quantity = 1, UnitPrice = 1m }]
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<SaleAlreadyCancelledException>();
    }
}
