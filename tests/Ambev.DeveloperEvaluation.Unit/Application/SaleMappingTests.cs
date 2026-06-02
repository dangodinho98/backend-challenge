using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SaleMappingTests
{
    private readonly IMapper _mapper;

    public SaleMappingTests()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<SaleMappingProfile>();
            cfg.AddProfile<SalesProfile>();
        });

        _mapper = config.CreateMapper();
    }

    [Fact(DisplayName = "Given sale entity When mapping to response Then preserves id")]
    public void Map_SaleToSaleResponse_PreservesId()
    {
        var sale = Sale.Create(
            "SALE-MAP-001",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 5, 10m)]);

        var model = _mapper.Map<SaleModel>(sale);
        var response = _mapper.Map<SaleResponse>(model);

        model.Id.Should().Be(sale.Id);
        response.Id.Should().Be(sale.Id);
    }
}
