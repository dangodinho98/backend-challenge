using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed class SaleCancelledEvent : IDomainEvent
{
    public Sale Sale { get; }
    public DateTime OccurredOn { get; }

    public SaleCancelledEvent(Sale sale)
    {
        Sale = sale;
        OccurredOn = DateTime.UtcNow;
    }
}
