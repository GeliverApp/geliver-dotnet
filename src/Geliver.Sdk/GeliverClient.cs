using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Geliver.Sdk.Models;

namespace Geliver.Sdk;

public class GeliverClient
{
    public const string DefaultBaseUrl = "https://api.geliver.io/api/v1";

    private readonly HttpClient _http;
    private readonly int _maxRetries;

    public ShipmentsResource Shipments { get; }
    public TransactionsResource Transactions { get; }
    public AddressesResource Addresses { get; }
    public WebhooksResource Webhooks { get; }
    public ProvidersResource Providers { get; }
    public ParcelTemplatesResource ParcelTemplates { get; }
    public PricesResource Prices { get; }
    public GeoResource Geo { get; }
    public OrganizationsResource Organizations { get; }

    public GeliverClient(string token, string? baseUrl = null, HttpClient? httpClient = null, int maxRetries = 2)
    {
        _http = httpClient ?? new HttpClient { BaseAddress = new Uri((baseUrl ?? DefaultBaseUrl).TrimEnd('/')) };
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _maxRetries = maxRetries;

        Shipments = new ShipmentsResource(this);
        Transactions = new TransactionsResource(this);
        Addresses = new AddressesResource(this);
        Webhooks = new WebhooksResource(this);
        Providers = new ProvidersResource(this);
        ParcelTemplates = new ParcelTemplatesResource(this);
        Prices = new PricesResource(this);
        Geo = new GeoResource(this);
        Organizations = new OrganizationsResource(this);
    }

    internal async Task<T?> RequestAsync<T>(HttpMethod method, string path, object? query = null, object? body = null, CancellationToken ct = default)
    {
        var url = path;
        if (query != null)
        {
            var pairs = from p in query.GetType().GetProperties()
                        let v = p.GetValue(query)
                        where v != null
                        select Uri.EscapeDataString(p.Name) + "=" + Uri.EscapeDataString(Convert.ToString(v)!);
            var qs = string.Join("&", pairs);
            if (!string.IsNullOrEmpty(qs)) url += "?" + qs;
        }
        var content = body != null ? new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json") : null;
        var req = new HttpRequestMessage(method, url) { Content = content };

        var attempt = 0;
        while (true)
        {
            var res = await _http.SendAsync(req, ct);
            var text = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode)
            {
                if (ShouldRetry((int)res.StatusCode) && attempt < _maxRetries)
                {
                    attempt++;
                    await Task.Delay(BackoffDelay(attempt), ct);
                    continue;
                }
                throw new HttpRequestException($"HTTP {(int)res.StatusCode}");
            }
            using var doc = JsonDocument.Parse(string.IsNullOrEmpty(text) ? "{}" : text);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, NumberHandling = JsonNumberHandling.AllowReadingFromString };
            options.Converters.Add(new JsonStringEnumConverter());
            if (doc.RootElement.TryGetProperty("data", out var data))
            {
                return data.Deserialize<T>(options);
            }
            return JsonSerializer.Deserialize<T>(text, options);
        }
    }

    private static bool ShouldRetry(int status) => status == 429 || status >= 500;
    private static TimeSpan BackoffDelay(int attempt)
    {
        var ms = Math.Min(2000, 200 * Math.Pow(2, attempt - 1));
        return TimeSpan.FromMilliseconds(ms);
    }
}

// Use generated models from Geliver.Sdk.Models instead of local records.

public static class Webhooks
{
    public static bool Verify(string body, IDictionary<string, string> headers, bool enableVerification = false, string? secret = null)
    {
        if (!enableVerification) return true;
        headers.TryGetValue("X-Signature", out var sig);
        headers.TryGetValue("x-signature", out var sig2);
        headers.TryGetValue("X-Timestamp", out var ts);
        headers.TryGetValue("x-timestamp", out var ts2);
        var signature = sig ?? sig2; var timestamp = ts ?? ts2;
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(secret)) return false;
        // Future: compute HMAC(secret, $"{timestamp}." + body) and compare
        return false;
    }
}

public class ShipmentsResource
{
    private readonly GeliverClient _c;
    internal ShipmentsResource(GeliverClient c) { _c = c; }

    public Task<Models.Shipment?> CreateAsync(object body, CancellationToken ct = default)
    {
        var dict = body.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(body));
        if (dict.TryGetValue("order", out var orderObj) && orderObj is Dictionary<string, object> orderDict)
        {
            if (!orderDict.ContainsKey("sourceCode") || string.IsNullOrEmpty(Convert.ToString(orderDict["sourceCode"])))
            {
                orderDict["sourceCode"] = "API";
            }
            dict["order"] = orderDict;
        }
        foreach (var k in new[] { "length", "width", "height", "weight" })
        {
            if (dict.TryGetValue(k, out var v) && v is not null) dict[k] = Convert.ToString(v)!;
        }
        return _c.RequestAsync<Models.Shipment>(HttpMethod.Post, "/shipments", null, dict, ct);
    }
    public Task<Models.Shipment?> GetAsync(string shipmentId, CancellationToken ct = default) => _c.RequestAsync<Models.Shipment>(HttpMethod.Get, $"/shipments/{Uri.EscapeDataString(shipmentId)}", null, null, ct);
    public Task<List<Models.Shipment>?> ListAsync(object? query = null, CancellationToken ct = default) => _c.RequestAsync<List<Models.Shipment>>(HttpMethod.Get, "/shipments", query, null, ct);

    /// <summary>List shipments returning full envelope (pagination info + data).</summary>
    public Task<ListEnvelope<List<Models.Shipment>>?> ListWithEnvelopeAsync(object? query = null, CancellationToken ct = default) => _c.RequestAsync<ListEnvelope<List<Models.Shipment>>>(HttpMethod.Get, "/shipments", query, null, ct);
    public Task<Models.Shipment?> UpdatePackageAsync(string shipmentId, object body, CancellationToken ct = default)
    {
        var dict = body.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(body));
        foreach (var k in new[] { "length", "width", "height", "weight" })
        {
            if (dict.TryGetValue(k, out var v) && v is not null) dict[k] = Convert.ToString(v)!;
        }
        return _c.RequestAsync<Models.Shipment>(HttpMethod.Patch, $"/shipments/{Uri.EscapeDataString(shipmentId)}", null, dict, ct);
    }
    public Task<Models.Shipment?> CancelAsync(string shipmentId, CancellationToken ct = default) => _c.RequestAsync<Models.Shipment>(HttpMethod.Delete, $"/shipments/{Uri.EscapeDataString(shipmentId)}", null, null, ct);
    public Task<Models.Shipment?> CloneAsync(string shipmentId, CancellationToken ct = default) => _c.RequestAsync<Models.Shipment>(HttpMethod.Post, $"/shipments/{Uri.EscapeDataString(shipmentId)}", null, null, ct);

    /// <summary>Create return shipment (PATCH with isReturn=true).</summary>
    public Task<Models.Shipment?> CreateReturnAsync(string shipmentId, object body, CancellationToken ct = default)
    {
        var dict = body.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(body));
        dict["isReturn"] = true;
        return _c.RequestAsync<Models.Shipment>(HttpMethod.Patch, $"/shipments/{Uri.EscapeDataString(shipmentId)}", null, dict, ct);
    }

    /// <summary>Create a test shipment by injecting test=true.</summary>
    public Task<Models.Shipment?> CreateTestAsync(object body, CancellationToken ct = default)
    {
        var dict = body.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(body));
        if (dict.TryGetValue("order", out var orderObj2) && orderObj2 is Dictionary<string, object> orderDict2)
        {
            if (!orderDict2.ContainsKey("sourceCode") || string.IsNullOrEmpty(Convert.ToString(orderDict2["sourceCode"])))
            {
                orderDict2["sourceCode"] = "API";
            }
            dict["order"] = orderDict2;
        }
        foreach (var k in new[] { "length", "width", "height", "weight" })
        {
            if (dict.TryGetValue(k, out var v) && v is not null) dict[k] = Convert.ToString(v)!;
        }
        dict["test"] = true;
        return _c.RequestAsync<Models.Shipment>(HttpMethod.Post, "/shipments", null, dict, ct);
    }


    public async Task<Models.Shipment?> WaitForTrackingNumberAsync(string shipmentId, TimeSpan? interval = null, TimeSpan? timeout = null, CancellationToken ct = default)
    {
        interval ??= TimeSpan.FromSeconds(3);
        timeout ??= TimeSpan.FromSeconds(180);
        var start = DateTime.UtcNow;
        while (true)
        {
            var s = await GetAsync(shipmentId, ct);
            if (!string.IsNullOrEmpty(s?.TrackingNumber)) return s;
            if (DateTime.UtcNow - start > timeout) throw new TimeoutException("Timed out waiting for tracking number");
            await Task.Delay(interval.Value, ct);
        }
    }
}

/// <summary>Envelope for list endpoints</summary>
public class ListEnvelope<T>
{
    public bool? Result { get; set; }
    public string? AdditionalMessage { get; set; }
    public int? Limit { get; set; }
    public int? Page { get; set; }
    public int? TotalRows { get; set; }
    public int? TotalPages { get; set; }
    public T? Data { get; set; }
}

public class TransactionsResource
{
    private readonly GeliverClient _c;
    internal TransactionsResource(GeliverClient c) { _c = c; }
    public Task<Models.Transaction?> AcceptOfferAsync(string offerId, CancellationToken ct = default) => _c.RequestAsync<Models.Transaction>(HttpMethod.Post, "/transactions", null, new { offerID = offerId }, ct);
}

public class AddressesResource
{
    private readonly GeliverClient _c;
    internal AddressesResource(GeliverClient c) { _c = c; }
    public Task<Dictionary<string, object>?> CreateAsync(object body, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Post, "/addresses", null, body, ct);
    public Task<Dictionary<string, object>?> CreateSenderAsync(object body, CancellationToken ct = default)
    {
        var dict = body.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(body));
        dict["isRecipientAddress"] = false;
        return _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Post, "/addresses", null, dict, ct);
    }
    public Task<Dictionary<string, object>?> CreateRecipientAsync(object body, CancellationToken ct = default)
    {
        var dict = body.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(body));
        dict["isRecipientAddress"] = true;
        return _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Post, "/addresses", null, dict, ct);
    }
    public Task<Dictionary<string, object>?> ListAsync(object? query = null, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Get, "/addresses", query, null, ct);
    public Task<List<Models.Address>?> ListTypedAsync(object? query = null, CancellationToken ct = default) => _c.RequestAsync<List<Models.Address>>(HttpMethod.Get, "/addresses", query, null, ct);
    public Task<ListEnvelope<List<Models.Address>>?> ListWithEnvelopeAsync(object? query = null, CancellationToken ct = default) => _c.RequestAsync<ListEnvelope<List<Models.Address>>>(HttpMethod.Get, "/addresses", query, null, ct);
    public Task<Dictionary<string, object>?> GetAsync(string addressId, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Get, $"/addresses/{Uri.EscapeDataString(addressId)}", null, null, ct);
    public Task<Dictionary<string, object>?> DeleteAsync(string addressId, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Delete, $"/addresses/{Uri.EscapeDataString(addressId)}", null, null, ct);
}

public class WebhooksResource
{
    private readonly GeliverClient _c;
    internal WebhooksResource(GeliverClient c) { _c = c; }
    public Task<Dictionary<string, object>?> CreateAsync(string url, string? type = null, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Post, "/webhook", null, type is null ? new { url } : new { url, type }, ct);
    public Task<Dictionary<string, object>?> ListAsync(CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Get, "/webhook", null, null, ct);
    public Task<List<Models.Webhook>?> ListTypedAsync(CancellationToken ct = default) => _c.RequestAsync<List<Models.Webhook>>(HttpMethod.Get, "/webhook", null, null, ct);
    public Task<ListEnvelope<List<Models.Webhook>>?> ListWithEnvelopeAsync(CancellationToken ct = default) => _c.RequestAsync<ListEnvelope<List<Models.Webhook>>>(HttpMethod.Get, "/webhook", null, null, ct);
    public Task<Dictionary<string, object>?> DeleteAsync(string webhookId, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Delete, $"/webhook/{Uri.EscapeDataString(webhookId)}", null, null, ct);
    public Task<Dictionary<string, object>?> TestAsync(string type, string url, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Put, "/webhook", null, new { type, url }, ct);
}

public partial class GeliverClient
{
    public Task<byte[]> DownloadLabelAsync(string url, CancellationToken ct = default) => _http.GetByteArrayAsync(url, ct);
    public async Task<byte[]> DownloadLabelForShipmentAsync(string shipmentId, CancellationToken ct = default)
    {
        var s = await Shipments.GetAsync(shipmentId, ct);
        var url = s?.LabelURL ?? throw new HttpRequestException("shipment has no labelURL");
        return await DownloadLabelAsync(url!, ct);
    }
    public Task<string> DownloadResponsiveLabelAsync(string url, CancellationToken ct = default) => _http.GetStringAsync(url, ct);
    public async Task<string> DownloadResponsiveLabelForShipmentAsync(string shipmentId, CancellationToken ct = default)
    {
        var s = await Shipments.GetAsync(shipmentId, ct);
        var url = (s as Dictionary<string, object>)?["responsiveLabelURL"]?.ToString() ?? (s as Dictionary<string, object>)?["responsiveLabelUrl"]?.ToString();
        if (string.IsNullOrEmpty(url)) throw new HttpRequestException("shipment has no responsiveLabelURL");
        return await DownloadResponsiveLabelAsync(url!, ct);
    }
}

public class ProvidersResource
{
    private readonly GeliverClient _c;
    internal ProvidersResource(GeliverClient c) { _c = c; }
    public Task<List<Models.ProviderAccount>?> ListAccountsAsync(CancellationToken ct = default) => _c.RequestAsync<List<Models.ProviderAccount>>(HttpMethod.Get, "/provideraccounts", null, null, ct);
    public Task<ListEnvelope<List<Models.ProviderAccount>>?> ListAccountsWithEnvelopeAsync(CancellationToken ct = default) => _c.RequestAsync<ListEnvelope<List<Models.ProviderAccount>>>(HttpMethod.Get, "/provideraccounts", null, null, ct);
    public Task<Models.ProviderAccount?> CreateAccountAsync(object body, CancellationToken ct = default) => _c.RequestAsync<Models.ProviderAccount>(HttpMethod.Post, "/provideraccounts", null, body, ct);
    public Task<Dictionary<string, object>?> DeleteAccountAsync(string providerAccountId, bool? isDeleteAccountConnection = null, CancellationToken ct = default)
    {
        var query = isDeleteAccountConnection is null ? null : new { isDeleteAccountConnection };
        return _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Delete, $"/provideraccounts/{Uri.EscapeDataString(providerAccountId)}", query, null, ct);
    }
}

public class ParcelTemplatesResource
{
    private readonly GeliverClient _c;
    internal ParcelTemplatesResource(GeliverClient c) { _c = c; }
    public Task<Models.ParcelTemplate?> CreateAsync(object body, CancellationToken ct = default) => _c.RequestAsync<Models.ParcelTemplate>(HttpMethod.Post, "/parceltemplates", null, body, ct);
    public Task<List<Models.ParcelTemplate>?> ListAsync(CancellationToken ct = default) => _c.RequestAsync<List<Models.ParcelTemplate>>(HttpMethod.Get, "/parceltemplates", null, null, ct);
    public Task<ListEnvelope<List<Models.ParcelTemplate>>?> ListWithEnvelopeAsync(CancellationToken ct = default) => _c.RequestAsync<ListEnvelope<List<Models.ParcelTemplate>>>(HttpMethod.Get, "/parceltemplates", null, null, ct);
    public Task<Dictionary<string, object>?> DeleteAsync(string templateId, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Delete, $"/parceltemplates/{Uri.EscapeDataString(templateId)}", null, null, ct);
}

public class PricesResource
{
    private readonly GeliverClient _c;
    internal PricesResource(GeliverClient c) { _c = c; }
    public Task<Dictionary<string, object>?> ListPricesAsync(object query, CancellationToken ct = default) => _c.RequestAsync<Dictionary<string, object>>(HttpMethod.Get, "/priceList", query, null, ct);
}

public class GeoResource
{
    private readonly GeliverClient _c;
    internal GeoResource(GeliverClient c) { _c = c; }
    public Task<List<Models.City>?> ListCitiesAsync(string countryCode, CancellationToken ct = default) => _c.RequestAsync<List<Models.City>>(HttpMethod.Get, "/cities", new { countryCode }, null, ct);
    public Task<List<Models.District>?> ListDistrictsAsync(string countryCode, string cityCode, CancellationToken ct = default) => _c.RequestAsync<List<Models.District>>(HttpMethod.Get, "/districts", new { countryCode, cityCode }, null, ct);
}

public class OrganizationsResource
{
    private readonly GeliverClient _c;
    internal OrganizationsResource(GeliverClient c) { _c = c; }
    public Task<Models.Organization?> GetBalanceAsync(string organizationId, CancellationToken ct = default) => _c.RequestAsync<Models.Organization>(HttpMethod.Get, $"/organizations/{Uri.EscapeDataString(organizationId)}/balance", null, null, ct);
}
