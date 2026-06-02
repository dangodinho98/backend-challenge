using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Exceptions;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    private readonly List<SaleItem> _items = [];
    private readonly List<IDomainEvent> _domainEvents = [];

    public string SaleNumber { get; private set; } = string.Empty;
    public DateTime SaleDate { get; private set; }
    public ExternalIdentity Customer { get; private set; } = null!;
    public ExternalIdentity Branch { get; private set; } = null!;
    public SaleStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Sale()
    {
    }

    public static Sale Create(
        string saleNumber,
        DateTime saleDate,
        ExternalIdentity customer,
        ExternalIdentity branch,
        IEnumerable<(ExternalIdentity Product, int Quantity, decimal UnitPrice)> items)
    {
        if (string.IsNullOrWhiteSpace(saleNumber))
            throw new DomainException("Sale number is required.");

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = saleNumber.Trim(),
            SaleDate = saleDate,
            Customer = customer,
            Branch = branch,
            Status = SaleStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        sale.ReplaceItems(items);
        sale.RaiseDomainEvent(new SaleCreatedEvent(sale));
        return sale;
    }

    public void Update(
        DateTime saleDate,
        ExternalIdentity customer,
        ExternalIdentity branch,
        IEnumerable<(ExternalIdentity Product, int Quantity, decimal UnitPrice)> items)
    {
        EnsureActive();

        SaleDate = saleDate;
        Customer = customer;
        Branch = branch;
        ReplaceItems(items);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SaleModifiedEvent(this));
    }

    public void Cancel()
    {
        if (Status == SaleStatus.Cancelled)
            throw new SaleAlreadyCancelledException(Id);

        Status = SaleStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        RecalculateTotal();
        RaiseDomainEvent(new SaleCancelledEvent(this));
    }

    public void CancelItem(Guid itemId)
    {
        EnsureActive();

        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Sale item '{itemId}' was not found.");

        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ItemCancelledEvent(this, item));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void ReplaceItems(IEnumerable<(ExternalIdentity Product, int Quantity, decimal UnitPrice)> items)
    {
        var itemList = items?.ToList() ?? throw new DomainException("Sale must contain at least one item.");

        if (itemList.Count == 0)
            throw new DomainException("Sale must contain at least one item.");

        _items.Clear();

        foreach (var (product, quantity, unitPrice) in itemList)
        {
            var saleItem = new SaleItem(product, quantity, unitPrice)
            {
                Id = Guid.NewGuid()
            };
            saleItem.AssignToSale(Id);
            _items.Add(saleItem);
        }

        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalAmount = Status == SaleStatus.Cancelled
            ? 0
            : Math.Round(_items.Where(i => !i.IsCancelled).Sum(i => i.LineTotal), 2, MidpointRounding.AwayFromZero);
    }

    private void EnsureActive()
    {
        if (Status == SaleStatus.Cancelled)
            throw new SaleAlreadyCancelledException(Id);
    }

    private void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
