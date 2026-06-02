using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IMapper _mapper;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CancelSaleItemHandler(_saleRepository, _domainEventDispatcher, _mapper);
    }

    [Fact(DisplayName = "Given multi-item sale When cancelling one item Then recalculates total")]
    public async Task Handle_MultiItemSale_CancelsItemAndRecalculatesTotal()
    {
        var product2 = new ExternalIdentity(Guid.NewGuid(), "Soda");
        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [
                (new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 2, 10m),
                (product2, 3, 5m)
            ]);

        sale.ClearDomainEvents();
        var itemToCancel = sale.Items.First().Id;

        _saleRepository.GetByIdForUpdateAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<SaleModel>(sale).Returns(new SaleModel { Id = sale.Id, TotalAmount = 15m });

        await _handler.Handle(new CancelSaleItemCommand { SaleId = sale.Id, ItemId = itemToCancel }, CancellationToken.None);

        sale.Items.First(i => i.Id == itemToCancel).IsCancelled.Should().BeTrue();
        sale.TotalAmount.Should().Be(15m);
        await _domainEventDispatcher.Received(1).DispatchAsync(
            Arg.Any<IEnumerable<Ambev.DeveloperEvaluation.Domain.Events.IDomainEvent>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When cancelling item Then throws KeyNotFoundException")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdForUpdateAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(
            new CancelSaleItemCommand { SaleId = saleId, ItemId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling item Then throws SaleAlreadyCancelledException")]
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

        var act = () => _handler.Handle(
            new CancelSaleItemCommand { SaleId = sale.Id, ItemId = sale.Items.First().Id },
            CancellationToken.None);

        await act.Should().ThrowAsync<SaleAlreadyCancelledException>();
    }

    [Fact(DisplayName = "Given unknown item When cancelling Then throws DomainException")]
    public async Task Handle_UnknownItem_ThrowsDomainException()
    {
        var sale = Sale.Create(
            "SALE-003",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 2, 10m)]);

        sale.ClearDomainEvents();
        _saleRepository.GetByIdForUpdateAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var act = () => _handler.Handle(
            new CancelSaleItemCommand { SaleId = sale.Id, ItemId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
