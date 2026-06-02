using Ambev.DeveloperEvaluation.Domain.Services;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Services;

public class DiscountPolicyTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 10)]
    [InlineData(9, 10)]
    [InlineData(10, 20)]
    [InlineData(20, 20)]
    public void GivenQuantity_WhenGetDiscountPercent_ThenReturnsExpected(int quantity, decimal expected)
    {
        var result = DiscountPolicy.GetDiscountPercent(quantity);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GivenQuantityAboveMax_WhenGetDiscountPercent_ThenThrows()
    {
        Assert.Throws<Ambev.DeveloperEvaluation.Domain.Exceptions.MaxQuantityExceededException>(() => DiscountPolicy.GetDiscountPercent(21));
    }

    [Fact]
    public void GivenZeroQuantity_WhenGetDiscountPercent_ThenThrows()
    {
        Assert.Throws<Ambev.DeveloperEvaluation.Domain.Exceptions.DomainException>(() => DiscountPolicy.GetDiscountPercent(0));
    }

    [Fact]
    public void GivenTwentyItemsAtTen_WhenCalculateLineTotal_ThenAppliesTwentyPercentDiscount()
    {
        var total = DiscountPolicy.CalculateLineTotal(20, 10m);
        Assert.Equal(160m, total);
    }

    [Fact]
    public void GivenFiveItemsAtTen_WhenCalculateLineTotal_ThenAppliesTenPercentDiscount()
    {
        var total = DiscountPolicy.CalculateLineTotal(5, 10m);
        Assert.Equal(45m, total);
    }
}
