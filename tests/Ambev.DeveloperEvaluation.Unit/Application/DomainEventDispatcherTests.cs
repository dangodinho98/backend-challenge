using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class DomainEventDispatcherTests
{
    private readonly IMediator _mediator;
    private readonly DomainEventDispatcher _dispatcher;

    public DomainEventDispatcherTests()
    {
        _mediator = Substitute.For<IMediator>();
        _dispatcher = new DomainEventDispatcher(_mediator);
    }

    [Fact(DisplayName = "Given SaleCreatedEvent When dispatching Then publishes SaleCreatedNotification")]
    public async Task DispatchAsync_SaleCreatedEvent_PublishesNotification()
    {
        var sale = CreateSale();

        await _dispatcher.DispatchAsync([new SaleCreatedEvent(sale)], CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<SaleCreatedNotification>(n => n.Sale == sale),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given SaleModifiedEvent When dispatching Then publishes SaleModifiedNotification")]
    public async Task DispatchAsync_SaleModifiedEvent_PublishesNotification()
    {
        var sale = CreateSale();

        await _dispatcher.DispatchAsync([new SaleModifiedEvent(sale)], CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<SaleModifiedNotification>(n => n.Sale == sale),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given SaleCancelledEvent When dispatching Then publishes SaleCancelledNotification")]
    public async Task DispatchAsync_SaleCancelledEvent_PublishesNotification()
    {
        var sale = CreateSale();

        await _dispatcher.DispatchAsync([new SaleCancelledEvent(sale)], CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<SaleCancelledNotification>(n => n.Sale == sale),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given ItemCancelledEvent When dispatching Then publishes ItemCancelledNotification")]
    public async Task DispatchAsync_ItemCancelledEvent_PublishesNotification()
    {
        var sale = CreateSale();
        var item = sale.Items.First();

        await _dispatcher.DispatchAsync([new ItemCancelledEvent(sale, item)], CancellationToken.None);

        await _mediator.Received(1).Publish(
            Arg.Is<ItemCancelledNotification>(n => n.Sale == sale && n.Item == item),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given multiple events When dispatching Then publishes each notification in order")]
    public async Task DispatchAsync_MultipleEvents_PublishesAll()
    {
        var sale = CreateSale();
        var item = sale.Items.First();

        await _dispatcher.DispatchAsync(
            [new SaleCreatedEvent(sale), new ItemCancelledEvent(sale, item)],
            CancellationToken.None);

        await _mediator.Received(2).Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    private static Sale CreateSale() =>
        Sale.Create(
            "SALE-EVT-001",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 5, 10m)]);
}
