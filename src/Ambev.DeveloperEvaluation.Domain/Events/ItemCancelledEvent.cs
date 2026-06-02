using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public sealed class ItemCancelledEvent : IDomainEvent
{
    public Sale Sale { get; }
    public SaleItem Item { get; }
    public DateTime OccurredOn { get; }

    public ItemCancelledEvent(Sale sale, SaleItem item)
    {
        Sale = sale;
        Item = item;
        OccurredOn = DateTime.UtcNow;
    }
}
