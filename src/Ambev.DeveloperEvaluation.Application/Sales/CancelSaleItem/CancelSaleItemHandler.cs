using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemCommand : IRequest<SaleModel>
{
    public Guid SaleId { get; set; }
    public Guid ItemId { get; set; }
}

public class CancelSaleItemCommandValidator : AbstractValidator<CancelSaleItemCommand>
{
    public CancelSaleItemCommandValidator()
    {
        RuleFor(x => x.SaleId).NotEmpty();
        RuleFor(x => x.ItemId).NotEmpty();
    }
}

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, SaleModel>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IMapper _mapper;

    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        IDomainEventDispatcher domainEventDispatcher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _domainEventDispatcher = domainEventDispatcher;
        _mapper = mapper;
    }

    public async Task<SaleModel> Handle(CancelSaleItemCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdForUpdateAsync(request.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale '{request.SaleId}' was not found.");

        sale.CancelItem(request.ItemId);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _domainEventDispatcher.DispatchAsync(sale.DomainEvents, cancellationToken);
        sale.ClearDomainEvents();

        return SaleMapper.ToModel(sale, _mapper);
    }
}
