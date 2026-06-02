using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ModifySale;

public class UpdateSaleItemCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class UpdateSaleCommand : IRequest<SaleModel>
{
    public Guid Id { get; set; }
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public IList<UpdateSaleItemCommand> Items { get; set; } = new List<UpdateSaleItemCommand>();
}

public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SaleDate).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.ProductName).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, SaleModel>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IMapper _mapper;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        IDomainEventDispatcher domainEventDispatcher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _domainEventDispatcher = domainEventDispatcher;
        _mapper = mapper;
    }

    public async Task<SaleModel> Handle(UpdateSaleCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale '{request.Id}' was not found.");

        sale.Update(
            request.SaleDate,
            new ExternalIdentity(request.CustomerId, request.CustomerName),
            new ExternalIdentity(request.BranchId, request.BranchName),
            request.Items.Select(i => (
                new ExternalIdentity(i.ProductId, i.ProductName),
                i.Quantity,
                i.UnitPrice)));

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _domainEventDispatcher.DispatchAsync(sale.DomainEvents, cancellationToken);
        sale.ClearDomainEvents();

        return SaleMapper.ToModel(sale, _mapper);
    }
}
