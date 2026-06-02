using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Builders;
using AutoMapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IMapper _mapper;
    private readonly CreateSaleHandler _handler;
    private readonly SaleTestDataBuilder _builder = new();

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateSaleHandler(_saleRepository, _domainEventDispatcher, _mapper);
    }

    [Fact(DisplayName = "Given valid command When creating Then persists sale and dispatches events")]
    public async Task Handle_ValidCommand_PersistsAndDispatchesEvents()
    {
        var command = _builder.BuildCreateCommand(quantity: 5, unitPrice: 10m);
        Sale? persisted = null;

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Do<Sale>(s => persisted = s), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());
        _mapper.Map<SaleModel>(Arg.Any<Sale>()).Returns(ci => new SaleModel { Id = ci.Arg<Sale>().Id });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().Be(persisted!.Id);
        persisted.TotalAmount.Should().Be(45m);
        await _domainEventDispatcher.Received(1).DispatchAsync(
            Arg.Any<IEnumerable<Ambev.DeveloperEvaluation.Domain.Events.IDomainEvent>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given duplicate sale number When creating Then throws ValidationException")]
    public async Task Handle_DuplicateSaleNumber_ThrowsValidationException()
    {
        var command = _builder.BuildCreateCommand(saleNumber: "SALE-DUP-001");
        var existing = _builder.BuildSale();

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns(existing);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact(DisplayName = "Given quantity above max When creating Then throws MaxQuantityExceededException")]
    public async Task Handle_QuantityAboveMax_ThrowsMaxQuantityExceededException()
    {
        var command = _builder.BuildCreateCommand(quantity: 21);

        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Ambev.DeveloperEvaluation.Domain.Exceptions.MaxQuantityExceededException>();
    }
}
