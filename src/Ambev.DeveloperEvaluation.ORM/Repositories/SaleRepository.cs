using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include("_items")
            .AsTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetBySaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include("_items")
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<(IReadOnlyList<Sale> Items, int TotalCount)> ListAsync(
        SaleListCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include("_items")
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.SaleNumber))
            query = query.Where(s => s.SaleNumber.Contains(StripWildcards(criteria.SaleNumber)));

        if (!string.IsNullOrWhiteSpace(criteria.Customer))
            query = query.Where(s => s.Customer.Description.Contains(StripWildcards(criteria.Customer)));

        if (!string.IsNullOrWhiteSpace(criteria.Branch))
            query = query.Where(s => s.Branch.Description.Contains(StripWildcards(criteria.Branch)));

        if (criteria.Status.HasValue)
            query = query.Where(s => s.Status == criteria.Status.Value);

        if (criteria.MinSaleDate.HasValue)
            query = query.Where(s => s.SaleDate >= criteria.MinSaleDate.Value);

        if (criteria.MaxSaleDate.HasValue)
            query = query.Where(s => s.SaleDate <= criteria.MaxSaleDate.Value);

        if (criteria.MinTotalAmount.HasValue)
            query = query.Where(s => s.TotalAmount >= criteria.MinTotalAmount.Value);

        if (criteria.MaxTotalAmount.HasValue)
            query = query.Where(s => s.TotalAmount <= criteria.MaxTotalAmount.Value);

        query = ApplyOrdering(query, criteria.Order);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((criteria.Page - 1) * criteria.Size)
            .Take(criteria.Size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string StripWildcards(string value) => value.Replace("*", string.Empty).Trim();

    private static IQueryable<Sale> ApplyOrdering(IQueryable<Sale> query, string? order)
    {
        if (string.IsNullOrWhiteSpace(order))
            return query.OrderByDescending(s => s.SaleDate);

        var parts = order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IOrderedQueryable<Sale>? ordered = null;

        foreach (var part in parts)
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var field = tokens[0].Trim('"').ToLowerInvariant();
            var descending = tokens.Length > 1 && tokens[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            ordered = field switch
            {
                "salenumber" => ApplyOrder(query, ordered, s => s.SaleNumber, descending),
                "saledate" => ApplyOrder(query, ordered, s => s.SaleDate, descending),
                "totalamount" => ApplyOrder(query, ordered, s => s.TotalAmount, descending),
                "status" => ApplyOrder(query, ordered, s => s.Status, descending),
                _ => ordered
            };

            query = ordered ?? query;
        }

        return ordered ?? query.OrderByDescending(s => s.SaleDate);
    }

    private static IOrderedQueryable<Sale> ApplyOrder<TKey>(
        IQueryable<Sale> query,
        IOrderedQueryable<Sale>? ordered,
        System.Linq.Expressions.Expression<Func<Sale, TKey>> keySelector,
        bool descending)
    {
        if (ordered is null)
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);

        return descending ? ordered.ThenByDescending(keySelector) : ordered.ThenBy(keySelector);
    }
}
