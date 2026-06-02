using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed class SaleModifiedEvent : IDomainEvent
{
    public Sale Sale { get; }
    public DateTime OccurredOn { get; }

    public SaleModifiedEvent(Sale sale)
    {
        Sale = sale;
        OccurredOn = DateTime.UtcNow;
    }
}
