using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public class DeleteSaleCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteSaleCommandValidator : AbstractValidator<DeleteSaleCommand>
{
    public DeleteSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, bool>
{
    private readonly Domain.Repositories.ISaleRepository _saleRepository;

    public DeleteSaleHandler(Domain.Repositories.ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<bool> Handle(DeleteSaleCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _saleRepository.DeleteAsync(request.Id, cancellationToken);
        if (!deleted)
            throw new KeyNotFoundException($"Sale '{request.Id}' was not found.");

        return true;
    }
}
