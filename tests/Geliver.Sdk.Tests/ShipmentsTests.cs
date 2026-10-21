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
    public async Task CreateIncomingShipmentSupportsAllAddressCombinations()
    {
        var handler = new CaptureOnceHandler();
        var http = new HttpClient(handler) { BaseAddress = new Uri(GeliverClient.DefaultBaseUrl) };
        var client = new GeliverClient("test", httpClient: http);
        var customer = new SenderAddressRequest { Name = "Customer", Phone = "+905051234567", Address1 = "A" };
        var merchant = new RecipientAddressRequest { Name = "Merchant", Phone = "+905051234568", Address1 = "B" };
        var cases = new (object Body, string SenderKey, string RecipientKey)[]
        {
            (new CreateShipmentWithRecipientID { IsReturn = true, SenderAddressID = "customer-1", RecipientAddressID = "merchant-1" }, "senderAddressID", "recipientAddressID"),
            (new CreateShipmentWithRecipientAddress { IsReturn = true, SenderAddressID = "customer-1", RecipientAddress = merchant }, "senderAddressID", "recipientAddress"),
            (new CreateShipmentWithRecipientID { IsReturn = true, SenderAddress = customer, RecipientAddressID = "merchant-1" }, "senderAddress", "recipientAddressID"),
            (new CreateShipmentWithRecipientAddress { IsReturn = true, SenderAddress = customer, RecipientAddress = merchant }, "senderAddress", "recipientAddress"),
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

        await client.Shipments.CreateAsync(new CreateShipmentWithRecipientID
        {
            IsReturn = true,
            SenderAddress = new SenderAddressRequest
            {
                Name = "Customer", Phone = "+905051234567", Address1 = "A",
                CountryCode = "TR", CityCode = "34", DistrictName = "Kadıköy",
            },
            RecipientAddressID = "merchant-1",
        });

        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.EndsWith("/shipments", handler.LastRequest.RequestUri!.AbsolutePath);
        using var doc = JsonDocument.Parse(handler.LastBody!);
        var body = doc.RootElement;
        Assert.True(body.GetProperty("isReturn").GetBoolean());
        Assert.Equal("Customer", body.GetProperty("senderAddress").GetProperty("name").GetString());
        Assert.Equal("merchant-1", body.GetProperty("recipientAddressID").GetString());
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
