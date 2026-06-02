using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Bogus;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

public class SalesLifecycleFunctionalTests : IClassFixture<SalesApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _client;
    private readonly Faker _faker = new();

    public SalesLifecycleFunctionalTests(SalesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact(DisplayName = "Given sales API When running full lifecycle Then all operations succeed")]
    public async Task SalesLifecycle_CreateGetUpdateListCancelItemCancelDelete_WorksEndToEnd()
    {
        var customerId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var saleNumber = $"SALE-{_faker.Random.AlphaNumeric(8).ToUpperInvariant()}";

        var createRequest = new CreateSaleRequest
        {
            SaleNumber = saleNumber,
            SaleDate = DateTime.UtcNow,
            Customer = new ExternalIdentityRequest { Id = customerId, Name = _faker.Company.CompanyName() },
            Branch = new ExternalIdentityRequest { Id = branchId, Name = _faker.Address.City() },
            Items =
            [
                new SaleItemRequest
                {
                    Product = new ExternalIdentityRequest { Id = product1Id, Name = "Beer 600ml" },
                    Quantity = 5,
                    UnitPrice = 10m
                },
                new SaleItemRequest
                {
                    Product = new ExternalIdentityRequest { Id = product2Id, Name = "Soda 350ml" },
                    Quantity = 3,
                    UnitPrice = 5m
                }
            ]
        };

        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleResponse>>(JsonOptions);
        created.Should().NotBeNull();
        created!.Success.Should().BeTrue();
        created.Data!.SaleNumber.Should().Be(saleNumber);
        created.Data.TotalAmount.Should().Be(60m);
        created.Data.Items.Should().HaveCount(2);

        var saleId = created.Data.Id;

        var getResponse = await _client.GetAsync($"/api/sales/{saleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleResponse>>(JsonOptions);
        fetched!.Data!.Id.Should().Be(saleId);

        var updateRequest = new UpdateSaleRequest
        {
            SaleDate = DateTime.UtcNow.AddDays(1),
            Customer = createRequest.Customer,
            Branch = createRequest.Branch,
            Items =
            [
                new SaleItemRequest
                {
                    Product = createRequest.Items[0].Product,
                    Quantity = 6,
                    UnitPrice = 10m
                },
                new SaleItemRequest
                {
                    Product = createRequest.Items[1].Product,
                    Quantity = 3,
                    UnitPrice = 5m
                }
            ]
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleResponse>>(JsonOptions);
        updated!.Data!.TotalAmount.Should().Be(69m);
        updated.Data.Items.Should().HaveCount(2);

        var listResponse = await _client.GetAsync($"/api/sales?SaleNumber={saleNumber}&_page=1&_size=10");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var list = await listResponse.Content.ReadFromJsonAsync<SaleListResponse>(JsonOptions);
        list!.Data.Should().ContainSingle(s => s.Id == saleId);

        var itemToCancel = updated.Data.Items.First().Id;
        var cancelItemResponse = await _client.PatchAsync($"/api/sales/{saleId}/items/{itemToCancel}/cancel", null);
        cancelItemResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterItemCancel = await cancelItemResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleResponse>>(JsonOptions);
        afterItemCancel!.Data!.Items.First(i => i.Id == itemToCancel).IsCancelled.Should().BeTrue();
        afterItemCancel.Data.TotalAmount.Should().Be(15m);

        var cancelSaleResponse = await _client.PatchAsync($"/api/sales/{saleId}/cancel", null);
        cancelSaleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var cancelled = await cancelSaleResponse.Content.ReadFromJsonAsync<ApiResponseWithData<SaleResponse>>(JsonOptions);
        cancelled!.Data!.Status.Should().Be("Cancelled");

        var deleteResponse = await _client.DeleteAsync($"/api/sales/{saleId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var missingResponse = await _client.GetAsync($"/api/sales/{saleId}");
        missingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Given quantity above max When creating sale Then returns bad request")]
    public async Task CreateSale_QuantityAboveMax_ReturnsBadRequest()
    {
        var request = new CreateSaleRequest
        {
            SaleNumber = $"SALE-{_faker.Random.AlphaNumeric(8).ToUpperInvariant()}",
            SaleDate = DateTime.UtcNow,
            Customer = new ExternalIdentityRequest { Id = Guid.NewGuid(), Name = "Acme" },
            Branch = new ExternalIdentityRequest { Id = Guid.NewGuid(), Name = "Main" },
            Items =
            [
                new SaleItemRequest
                {
                    Product = new ExternalIdentityRequest { Id = Guid.NewGuid(), Name = "Beer" },
                    Quantity = 21,
                    UnitPrice = 10m
                }
            ]
        };

        var response = await _client.PostAsJsonAsync("/api/sales", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
