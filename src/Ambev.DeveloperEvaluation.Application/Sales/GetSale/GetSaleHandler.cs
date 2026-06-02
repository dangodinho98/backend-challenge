using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleQuery : IRequest<SaleModel>
{
    public Guid Id { get; set; }
}

public class GetSaleQueryValidator : AbstractValidator<GetSaleQuery>
{
    public GetSaleQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class GetSaleHandler : IRequestHandler<GetSaleQuery, SaleModel>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;

    public GetSaleHandler(ISaleRepository saleRepository, IMapper mapper)
    {
        _saleRepository = saleRepository;
        _mapper = mapper;
    }

    public async Task<SaleModel> Handle(GetSaleQuery request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale '{request.Id}' was not found.");

        return SaleMapper.ToModel(sale, _mapper);
    }
}
