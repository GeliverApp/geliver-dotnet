using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Geliver.Sdk;
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

public class ShipmentsTests
{
    [Fact]
    public async Task ListShipments_ReturnsData()
    {
        var http = new HttpClient(new FakeHandler()) { BaseAddress = new System.Uri(GeliverClient.DefaultBaseUrl) };
        var client = new GeliverClient("test", httpClient: http);
        var resp = await client.Shipments.ListAsync();
        Assert.NotNull(resp);
    }
}

