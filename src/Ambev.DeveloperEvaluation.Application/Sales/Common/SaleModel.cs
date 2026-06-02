namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

public class ExternalIdentityModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class SaleItemModel
{
    public Guid Id { get; set; }
    public ExternalIdentityModel Product { get; set; } = new();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal LineTotal { get; set; }
    public bool IsCancelled { get; set; }
}

public class SaleModel
{
    public Guid Id { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public ExternalIdentityModel Customer { get; set; } = new();
    public ExternalIdentityModel Branch { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public IList<SaleItemModel> Items { get; set; } = new List<SaleItemModel>();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SaleListModel
{
    public IList<SaleModel> Data { get; set; } = new List<SaleModel>();
    public int TotalItems { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
