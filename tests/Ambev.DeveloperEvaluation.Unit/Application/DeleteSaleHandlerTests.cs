using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class DeleteSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly DeleteSaleHandler _handler;

    public DeleteSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _handler = new DeleteSaleHandler(_saleRepository);
    }

    [Fact(DisplayName = "Given existing sale When deleting Then returns true")]
    public async Task Handle_ExistingSale_ReturnsTrue()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.DeleteAsync(saleId, Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(new DeleteSaleCommand { Id = saleId }, CancellationToken.None);

        result.Should().BeTrue();
        await _saleRepository.Received(1).DeleteAsync(saleId, Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When deleting Then throws KeyNotFoundException")]
    public async Task Handle_MissingSale_ThrowsKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.DeleteAsync(saleId, Arg.Any<CancellationToken>()).Returns(false);

        var act = () => _handler.Handle(new DeleteSaleCommand { Id = saleId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
