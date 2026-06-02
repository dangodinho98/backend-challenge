using Ambev.DeveloperEvaluation.WebApi.Features.Sales;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

public class CreateSaleRequest
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public ExternalIdentityRequest Customer { get; set; } = new();
    public ExternalIdentityRequest Branch { get; set; } = new();
    public IList<SaleItemRequest> Items { get; set; } = new List<SaleItemRequest>();
}

public class CreateSaleRequestValidator : AbstractValidator<CreateSaleRequest>
{
    public CreateSaleRequestValidator()
    {
        RuleFor(x => x.SaleNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SaleDate).NotEmpty();
        RuleFor(x => x.Customer.Id).NotEmpty();
        RuleFor(x => x.Customer.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Branch.Id).NotEmpty();
        RuleFor(x => x.Branch.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Product.Id).NotEmpty();
            item.RuleFor(i => i.Product.Name).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}
