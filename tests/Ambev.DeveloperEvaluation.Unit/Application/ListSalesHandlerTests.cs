using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new ListSalesHandler(_saleRepository, _mapper);
    }

    [Fact(DisplayName = "Given sales exist When listing Then returns paginated result")]
    public async Task Handle_SalesExist_ReturnsPaginatedResult()
    {
        var sale = Sale.Create(
            "SALE-001",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 5, 10m)]);

        var saleModel = new SaleModel { Id = sale.Id, SaleNumber = "SALE-001" };

        _saleRepository
            .ListAsync(Arg.Any<SaleListCriteria>(), Arg.Any<CancellationToken>())
            .Returns((new List<Sale> { sale } as IReadOnlyList<Sale>, 1));

        _mapper.Map<SaleModel>(sale).Returns(saleModel);

        var query = new ListSalesQuery { Page = 1, Size = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Data.Should().ContainSingle().Which.Should().BeEquivalentTo(saleModel);
        result.TotalItems.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact(DisplayName = "Given no sales When listing Then returns empty paginated result")]
    public async Task Handle_NoSales_ReturnsEmptyResult()
    {
        _saleRepository
            .ListAsync(Arg.Any<SaleListCriteria>(), Arg.Any<CancellationToken>())
            .Returns((Array.Empty<Sale>(), 0));

        var result = await _handler.Handle(new ListSalesQuery { Page = 1, Size = 10 }, CancellationToken.None);

        result.Data.Should().BeEmpty();
        result.TotalItems.Should().Be(0);
        result.TotalPages.Should().Be(0);
    }
}
