using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

public class ListSalesQuery : IRequest<SaleListModel>
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Order { get; set; }
    public string? SaleNumber { get; set; }
    public string? Customer { get; set; }
    public string? Branch { get; set; }
    public string? Status { get; set; }
    public DateTime? MinSaleDate { get; set; }
    public DateTime? MaxSaleDate { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }
}

public class ListSalesQueryValidator : AbstractValidator<ListSalesQuery>
{
    public ListSalesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Size).InclusiveBetween(1, 100);
    }
}

public class ListSalesHandler : IRequestHandler<ListSalesQuery, SaleListModel>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public ListSalesHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<SaleListModel> Handle(ListSalesQuery request, CancellationToken cancellationToken)
    {
        SaleStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<SaleStatus>(request.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var criteria = new SaleListCriteria
        {
            Page = request.Page,
            Size = request.Size,
            Order = request.Order,
            SaleNumber = request.SaleNumber,
            Customer = request.Customer,
            Branch = request.Branch,
            Status = status,
            MinSaleDate = request.MinSaleDate,
            MaxSaleDate = request.MaxSaleDate,
            MinTotalAmount = request.MinTotalAmount,
            MaxTotalAmount = request.MaxTotalAmount
        };

        var (items, totalCount) = await _saleRepository.ListAsync(criteria, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.Size);

        return new SaleListModel
        {
            Data = items.Select(s => SaleMapper.ToModel(s, _mapper)).ToList(),
            TotalItems = totalCount,
            CurrentPage = request.Page,
            TotalPages = totalPages
        };
    }
}
