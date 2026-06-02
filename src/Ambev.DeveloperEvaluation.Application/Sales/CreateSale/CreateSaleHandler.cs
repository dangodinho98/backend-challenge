using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleItemCommand
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class CreateSaleCommand : IRequest<SaleModel>
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public IList<CreateSaleItemCommand> Items { get; set; } = new List<CreateSaleItemCommand>();
}

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.SaleNumber).NotEmpty().MaximumLength(50);
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

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, SaleModel>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IMapper _mapper;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        IDomainEventDispatcher domainEventDispatcher,
        IMapper mapper)
    {
        _saleRepository = saleRepository;
        _domainEventDispatcher = domainEventDispatcher;
        _mapper = mapper;
    }

    public async Task<SaleModel> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _saleRepository.GetBySaleNumberAsync(request.SaleNumber, cancellationToken);
        if (existing is not null)
            throw new ValidationException(new[] { new FluentValidation.Results.ValidationFailure("SaleNumber", $"Sale number '{request.SaleNumber}' already exists.") });

        var sale = Sale.Create(
            request.SaleNumber,
            request.SaleDate,
            new ExternalIdentity(request.CustomerId, request.CustomerName),
            new ExternalIdentity(request.BranchId, request.BranchName),
            request.Items.Select(i => (
                new ExternalIdentity(i.ProductId, i.ProductName),
                i.Quantity,
                i.UnitPrice)));

        await _saleRepository.CreateAsync(sale, cancellationToken);
        await _domainEventDispatcher.DispatchAsync(sale.DomainEvents, cancellationToken);
        sale.ClearDomainEvents();

        return SaleMapper.ToModel(sale, _mapper);
    }
}
