using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSale;

public class CancelSaleCommand : IRequest<SaleModel>
{
    public Guid Id { get; set; }
}

public class CancelSaleCommandValidator : AbstractValidator<CancelSaleCommand>
{
    public CancelSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, SaleModel>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IMapper _mapper;

    public CancelSaleHandler(
        ISaleRepository saleRepository,
        IDomainEventDispatcher domainEventDispatcher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _domainEventDispatcher = domainEventDispatcher;
        _mapper = mapper;
    }

    public async Task<SaleModel> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale '{request.Id}' was not found.");

        sale.Cancel();

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _domainEventDispatcher.DispatchAsync(sale.DomainEvents, cancellationToken);
        sale.ClearDomainEvents();

        return SaleMapper.ToModel(sale, _mapper);
    }
}
