using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
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

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IMapper _mapper;
    private readonly CancelSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CancelSaleHandler(_saleRepository, _domainEventDispatcher, _mapper);
    }

    [Fact(DisplayName = "Given active sale When cancelling Then returns cancelled sale")]
    public async Task Handle_ActiveSale_ReturnsCancelledSale()
    {
        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 5, 10m)]);

        sale.ClearDomainEvents();

        var expected = new SaleModel { Id = sale.Id, Status = SaleStatus.Cancelled.ToString(), TotalAmount = 0 };

        _saleRepository.GetByIdForUpdateAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        _saleRepository.UpdateAsync(sale, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<SaleModel>(sale).Returns(expected);

        var result = await _handler.Handle(new CancelSaleCommand { Id = sale.Id }, CancellationToken.None);

        result.Status.Should().Be(SaleStatus.Cancelled.ToString());
        result.TotalAmount.Should().Be(0);
        await _domainEventDispatcher.Received(1).DispatchAsync(
            Arg.Any<IEnumerable<Ambev.DeveloperEvaluation.Domain.Events.IDomainEvent>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When cancelling Then throws KeyNotFoundException")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdForUpdateAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new CancelSaleCommand { Id = saleId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given cancelled sale When cancelling again Then throws SaleAlreadyCancelledException")]
    public async Task Handle_AlreadyCancelled_ThrowsSaleAlreadyCancelledException()
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

        var act = () => _handler.Handle(new CancelSaleCommand { Id = sale.Id }, CancellationToken.None);

        await act.Should().ThrowAsync<SaleAlreadyCancelledException>();
    }
}
