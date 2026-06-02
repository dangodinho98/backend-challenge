using Ambev.DeveloperEvaluation.Application.Events;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class SaleEventNotificationHandlerTests
{
    [Fact(DisplayName = "Given SaleCreatedNotification When handling Then logs information")]
    public async Task SaleCreatedNotificationHandler_LogsInformation()
    {
        var sale = CreateSale();
        var logger = Substitute.For<ILogger<SaleCreatedNotificationHandler>>();
        var handler = new SaleCreatedNotificationHandler(logger);

        await handler.Handle(new SaleCreatedNotification(sale), CancellationToken.None);

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(DisplayName = "Given SaleModifiedNotification When handling Then logs information")]
    public async Task SaleModifiedNotificationHandler_LogsInformation()
    {
        var sale = CreateSale();
        var logger = Substitute.For<ILogger<SaleModifiedNotificationHandler>>();
        var handler = new SaleModifiedNotificationHandler(logger);

        await handler.Handle(new SaleModifiedNotification(sale), CancellationToken.None);

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(DisplayName = "Given SaleCancelledNotification When handling Then logs information")]
    public async Task SaleCancelledNotificationHandler_LogsInformation()
    {
        var sale = CreateSale();
        var logger = Substitute.For<ILogger<SaleCancelledNotificationHandler>>();
        var handler = new SaleCancelledNotificationHandler(logger);

        await handler.Handle(new SaleCancelledNotification(sale), CancellationToken.None);

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact(DisplayName = "Given ItemCancelledNotification When handling Then logs information")]
    public async Task ItemCancelledNotificationHandler_LogsInformation()
    {
        var sale = CreateSale();
        var item = sale.Items.First();
        var logger = Substitute.For<ILogger<ItemCancelledNotificationHandler>>();
        var handler = new ItemCancelledNotificationHandler(logger);

        await handler.Handle(new ItemCancelledNotification(sale, item), CancellationToken.None);

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    private static Sale CreateSale() =>
        Sale.Create(
            "SALE-LOG-001",
            DateTime.UtcNow,
            new ExternalIdentity(Guid.NewGuid(), "Acme Corp"),
            new ExternalIdentity(Guid.NewGuid(), "Downtown"),
            [(new ExternalIdentity(Guid.NewGuid(), "Beer 600ml"), 5, 10m)]);
}
