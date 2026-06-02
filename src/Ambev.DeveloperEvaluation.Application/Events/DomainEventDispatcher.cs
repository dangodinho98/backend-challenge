using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Events;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            switch (domainEvent)
            {
                case SaleCreatedEvent e:
                    await _mediator.Publish(new SaleCreatedNotification(e.Sale), cancellationToken);
                    break;
                case SaleModifiedEvent e:
                    await _mediator.Publish(new SaleModifiedNotification(e.Sale), cancellationToken);
                    break;
                case SaleCancelledEvent e:
                    await _mediator.Publish(new SaleCancelledNotification(e.Sale), cancellationToken);
                    break;
                case ItemCancelledEvent e:
                    await _mediator.Publish(new ItemCancelledNotification(e.Sale, e.Item), cancellationToken);
                    break;
            }
        }
    }
}

public record SaleCreatedNotification(Domain.Entities.Sale Sale) : INotification;
public record SaleModifiedNotification(Domain.Entities.Sale Sale) : INotification;
public record SaleCancelledNotification(Domain.Entities.Sale Sale) : INotification;
public record ItemCancelledNotification(Domain.Entities.Sale Sale, Domain.Entities.SaleItem Item) : INotification;

public class SaleCreatedNotificationHandler : INotificationHandler<SaleCreatedNotification>
{
    private readonly ILogger<SaleCreatedNotificationHandler> _logger;

    public SaleCreatedNotificationHandler(ILogger<SaleCreatedNotificationHandler> logger) => _logger = logger;

    public Task Handle(SaleCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "SaleCreated: SaleId={SaleId}, SaleNumber={SaleNumber}, TotalAmount={TotalAmount}",
            notification.Sale.Id, notification.Sale.SaleNumber, notification.Sale.TotalAmount);
        return Task.CompletedTask;
    }
}

public class SaleModifiedNotificationHandler : INotificationHandler<SaleModifiedNotification>
{
    private readonly ILogger<SaleModifiedNotificationHandler> _logger;

    public SaleModifiedNotificationHandler(ILogger<SaleModifiedNotificationHandler> logger) => _logger = logger;

    public Task Handle(SaleModifiedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "SaleModified: SaleId={SaleId}, SaleNumber={SaleNumber}, TotalAmount={TotalAmount}",
            notification.Sale.Id, notification.Sale.SaleNumber, notification.Sale.TotalAmount);
        return Task.CompletedTask;
    }
}

public class SaleCancelledNotificationHandler : INotificationHandler<SaleCancelledNotification>
{
    private readonly ILogger<SaleCancelledNotificationHandler> _logger;

    public SaleCancelledNotificationHandler(ILogger<SaleCancelledNotificationHandler> logger) => _logger = logger;

    public Task Handle(SaleCancelledNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "SaleCancelled: SaleId={SaleId}, SaleNumber={SaleNumber}",
            notification.Sale.Id, notification.Sale.SaleNumber);
        return Task.CompletedTask;
    }
}

public class ItemCancelledNotificationHandler : INotificationHandler<ItemCancelledNotification>
{
    private readonly ILogger<ItemCancelledNotificationHandler> _logger;

    public ItemCancelledNotificationHandler(ILogger<ItemCancelledNotificationHandler> logger) => _logger = logger;

    public Task Handle(ItemCancelledNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ItemCancelled: SaleId={SaleId}, ItemId={ItemId}, Product={Product}",
            notification.Sale.Id, notification.Item.Id, notification.Item.Product.Description);
        return Task.CompletedTask;
    }
}
