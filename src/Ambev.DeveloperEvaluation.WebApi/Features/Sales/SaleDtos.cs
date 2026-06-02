namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales;

public class ExternalIdentityRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ExternalIdentityResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SaleItemRequest
{
    public ExternalIdentityRequest Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SaleItemResponse
{
    public Guid Id { get; set; }
    public ExternalIdentityResponse Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsCancelled { get; set; }
}

public class SaleResponse
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public ExternalIdentityResponse Customer { get; set; } = new();
    public ExternalIdentityResponse Branch { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public IList<SaleItemResponse> Items { get; set; } = new List<SaleItemResponse>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SaleListResponse
{
    public IList<SaleResponse> Data { get; set; } = new List<SaleResponse>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}

public class ListSalesRequest
{
    public int _page { get; set; } = 1;
    public int _size { get; set; } = 10;
    public string? _order { get; set; }
    public string? SaleNumber { get; set; }
    public string? Customer { get; set; }
    public string? Branch { get; set; }
    public string? Status { get; set; }
    public DateTime? _minSaleDate { get; set; }
    public DateTime? _maxSaleDate { get; set; }
    public decimal? _minTotalAmount { get; set; }
    public decimal? _maxTotalAmount { get; set; }
}
