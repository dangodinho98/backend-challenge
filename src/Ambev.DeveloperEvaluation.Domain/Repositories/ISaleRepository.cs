using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public class SaleListCriteria
{
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string? Order { get; init; }
    public string? SaleNumber { get; init; }
    public string? Customer { get; init; }
    public string? Branch { get; init; }
    public SaleStatus? Status { get; init; }
    public DateTime? MinSaleDate { get; init; }
    public DateTime? MaxSaleDate { get; init; }
    public decimal? MinTotalAmount { get; init; }
    public decimal? MaxTotalAmount { get; init; }
}

public interface ISaleRepository
{
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Sale> Items, int TotalCount)> ListAsync(SaleListCriteria criteria, CancellationToken cancellationToken = default);
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
