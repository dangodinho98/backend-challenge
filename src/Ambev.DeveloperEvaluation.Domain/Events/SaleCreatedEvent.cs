using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed class SaleCreatedEvent : IDomainEvent
{
    public Sale Sale { get; }
    public DateTime OccurredOn { get; }

    public SaleCreatedEvent(Sale sale)
    {
        Sale = sale;
        OccurredOn = DateTime.UtcNow;
    }
}
