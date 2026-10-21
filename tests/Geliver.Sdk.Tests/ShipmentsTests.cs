using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Geliver.Sdk;
using Geliver.Sdk.Models;
using Xunit;

class FakeHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri!.AbsolutePath;
        // Handle both absolute and relative paths
        if (path.EndsWith("/shipments") || path == "/shipments")
        {
            var json = "{\"result\":true, \"data\":[{\"id\":\"s1\"}]}";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){ Content = new StringContent(json) });
        }
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

class CaptureOnceHandler : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastBody { get; private set; }
    public string ResponseJson { get; set; } = "{\"result\":true, \"data\":{\"id\":\"ret-1\"}}";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastBody = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(ResponseJson) };
    }
}

public class ShipmentsTests
{
    [Fact]
    public async Task NewShipmentFlowsRequireRecipientAndAllowDefaultSender()
    {
        var client = new GeliverClient("test");
        var invalid = new object[]
        {
            new CreateShipmentWithRecipientID { SenderAddressID = "merchant-1", SenderAddress = new SenderAddressRequest { Address1 = "A" }, RecipientAddressID = "customer-1" },
            new CreateShipmentWithRecipientID { SenderAddressID = "merchant-1" },
            new { SenderAddressID = "merchant-1", RecipientAddressID = "customer-1", RecipientAddress = new RecipientAddressRequest { Address1 = "B" } },
        };

        foreach (var body in invalid)
        {
            await Assert.ThrowsAsync<ArgumentException>(() => client.Shipments.CreateAsync(body));
            await Assert.ThrowsAsync<ArgumentException>(() => client.Shipments.CreateTestAsync(body));
            await Assert.ThrowsAsync<ArgumentException>(() => client.Transactions.CreateAsync(new { Shipment = body }));
        }
    }

    [Fact]
    public async Task CreateIncomingShipmentSupportsAllAddressCombinations()
    {
        var handler = new CaptureOnceHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri(GeliverClient.DefaultBaseUrl) };
        var client = new GeliverClient("test", httpClient: http);
        var merchant = new SenderAddressRequest { Name = "Merchant", Phone = "+905051234567", Address1 = "A" };
        var customer = new RecipientAddressRequest { Name = "Customer", Phone = "+905051234568", Address1 = "B" };
        var cases = new (object Body, string SenderKey, string RecipientKey)[]
        {
            (new CreateShipmentWithRecipientAddress { IsReturn = true, RecipientAddress = customer }, "", "recipientAddress"),
            (new CreateShipmentWithRecipientID { IsReturn = true, SenderAddressID = "merchant-1", RecipientAddressID = "customer-1" }, "senderAddressID", "recipientAddressID"),
            (new CreateShipmentWithRecipientAddress { IsReturn = true, SenderAddressID = "merchant-1", RecipientAddress = customer }, "senderAddressID", "recipientAddress"),
            (new CreateShipmentWithRecipientID { IsReturn = true, SenderAddress = merchant, RecipientAddressID = "customer-1" }, "senderAddress", "recipientAddressID"),
            (new CreateShipmentWithRecipientAddress { IsReturn = true, SenderAddress = merchant, RecipientAddress = customer }, "senderAddress", "recipientAddress"),
        };

        foreach (var (request, senderKey, recipientKey) in cases)
        {
            await client.Shipments.CreateAsync(request);
            using var doc = JsonDocument.Parse(handler.LastBody!);
            var body = doc.RootElement;
            Assert.True(body.GetProperty("isReturn").GetBoolean());
            foreach (var key in new[] { "senderAddressID", "senderAddress", "recipientAddressID", "recipientAddress" })
            {
                Assert.Equal(key == senderKey || key == recipientKey, body.TryGetProperty(key, out var value) && value.ValueKind != JsonValueKind.Null);
            }
        }
    }

    [Fact]
    public async Task CreateIncomingShipmentKeepsAddressDirection()
    {
        var handler = new CaptureOnceHandler();
        var http = new HttpClient(handler) { BaseAddress = new System.Uri(GeliverClient.DefaultBaseUrl) };
        var client = new GeliverClient("test", httpClient: http);

        await client.Shipments.CreateAsync(new CreateShipmentWithRecipientAddress
        {
            IsReturn = true,
            RecipientAddress = new RecipientAddressRequest
            {
                Name = "Customer", Phone = "+905051234567", Address1 = "A",
                CountryCode = "TR", CityCode = "34", DistrictName = "Kadıköy",
            },
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/shipments", handler.LastRequest.RequestUri!.AbsolutePath);
        using var doc = JsonDocument.Parse(handler.LastBody!);
        var body = doc.RootElement;
        Assert.True(body.GetProperty("isReturn").GetBoolean());
        Assert.Equal("Customer", body.GetProperty("recipientAddress").GetProperty("name").GetString());
        Assert.False(body.TryGetProperty("senderAddress", out var senderAddress) && senderAddress.ValueKind != JsonValueKind.Null);
        Assert.False(body.TryGetProperty("senderAddressID", out var senderID) && senderID.ValueKind != JsonValueKind.Null);
    }

    [Fact]
    public async Task ListShipments_ReturnsData()
    {
        var http = new HttpClient(new FakeHandler()) { BaseAddress = new System.Uri(GeliverClient.DefaultBaseUrl) };
        var client = new GeliverClient("test", httpClient: http);
        var resp = await client.Shipments.ListAsync();
        Assert.NotNull(resp);
    }

    [Fact]
    public async Task CreateReturn_UsesPostAndDefaults()
    {
        var handler = new CaptureOnceHandler();
        var http = new HttpClient(handler) { BaseAddress = new System.Uri(GeliverClient.DefaultBaseUrl) };
        var client = new GeliverClient("test", httpClient: http);

        var returned = await client.Shipments.CreateReturnAsync("shp-1", new { providerServiceCode = "SURAT_STANDART" });

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/shipments/shp-1", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Contains($"geliver-csharp/{GeliverClient.Version}", handler.LastRequest!.Headers.UserAgent.ToString());

        Assert.False(string.IsNullOrEmpty(handler.LastBody));
        using var doc = JsonDocument.Parse(handler.LastBody!);
        var root = doc.RootElement;
        Assert.True(root.GetProperty("isReturn").GetBoolean());
        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.Equal("SURAT_STANDART", root.GetProperty("providerServiceCode").GetString());
        Assert.False(root.TryGetProperty("willAccept", out _));

        Assert.NotNull(returned);
        Assert.Equal("ret-1", returned!.Id);
    }

    [Fact]
    public async Task CreateReturn_WithWillAccept_Throws()
    {
        var handler = new CaptureOnceHandler();
        var http = new HttpClient(handler) { BaseAddress = new System.Uri(GeliverClient.DefaultBaseUrl) };
        var client = new GeliverClient("test", httpClient: http);

        await Assert.ThrowsAsync<ArgumentException>(() => client.Shipments.CreateReturnAsync("shp-1", new { willAccept = true }));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ReturnFlowsPreserveCustomerSenderAddress(bool purchase)
    {
        var handler = new CaptureOnceHandler();
        var client = new GeliverClient("test", httpClient: new HttpClient(handler));
        var request = new { senderAddress = new { name = "Customer", phone = "+905051234567", address1 = "Pickup address" } };
        if (purchase)
            await client.Transactions.CreateReturnAsync("original-1", request);
        else
            await client.Shipments.CreateReturnAsync("original-1", request);

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/shipments/original-1", handler.LastRequest.RequestUri!.AbsolutePath);
        using var doc = JsonDocument.Parse(handler.LastBody!);
        var body = doc.RootElement;
        Assert.Equal("Customer", body.GetProperty("senderAddress").GetProperty("name").GetString());
        Assert.Equal("Pickup address", body.GetProperty("senderAddress").GetProperty("address1").GetString());
        Assert.False(body.TryGetProperty("recipientAddress", out _));
        Assert.False(body.TryGetProperty("recipientAddressID", out _));
        Assert.True(body.GetProperty("isReturn").GetBoolean());
        Assert.Equal(purchase, body.TryGetProperty("willAccept", out var willAccept) && willAccept.GetBoolean());
    }

    [Fact]
    public async Task TransactionsCreateReturn_ForcesWillAcceptAndReturnsTransaction()
    {
        var handler = new CaptureOnceHandler
        {
            ResponseJson = "{\"result\":true, \"data\":{\"id\":\"tx-1\",\"offerID\":\"offer-1\",\"transactionType\":\"CREATE_SHIPMENT\"}}",
        };
        var http = new HttpClient(handler) { BaseAddress = new System.Uri(GeliverClient.DefaultBaseUrl) };
        var client = new GeliverClient("test", httpClient: http);

        var returned = await client.Transactions.CreateReturnAsync("shp-1", new { providerServiceCode = "SURAT_STANDART" });

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/shipments/shp-1", handler.LastRequest!.RequestUri!.AbsolutePath);

        Assert.False(string.IsNullOrEmpty(handler.LastBody));
        using var doc = JsonDocument.Parse(handler.LastBody!);
        var root = doc.RootElement;
        Assert.True(root.GetProperty("isReturn").GetBoolean());
        Assert.True(root.GetProperty("willAccept").GetBoolean());
        Assert.Equal(1, root.GetProperty("count").GetInt32());
        Assert.Equal("SURAT_STANDART", root.GetProperty("providerServiceCode").GetString());

        Assert.NotNull(returned);
        Assert.Equal("tx-1", returned!.Id);
        Assert.Equal("offer-1", returned.OfferID);
    }
}
