using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given existing sale When getting by id Then returns sale model")]
    public async Task Handle_ExistingSale_ReturnsSaleModel()
    {
        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 5, 10m)]);

        var saleId = sale.Id;
        var expected = new SaleModel { Id = saleId, SaleNumber = "SALE-001" };

        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns(sale);
        _mapper.Map<SaleModel>(sale).Returns(expected);

        var result = await _handler.Handle(new GetSaleQuery { Id = saleId }, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        await _saleRepository.Received(1).GetByIdAsync(saleId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When getting by id Then throws KeyNotFoundException")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var act = () => _handler.Handle(new GetSaleQuery { Id = saleId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{saleId}*");
    }
}
