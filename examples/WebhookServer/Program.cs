using System.Linq;
using System.Text.Json;
using Geliver.Sdk.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/webhooks/geliver", async (HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    var headers = req.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value));
    if (!Geliver.Sdk.Webhooks.Verify(body, headers, enableVerification: false))
    {
        return Results.BadRequest(new { status = "invalid" });
    }
    var evt = JsonSerializer.Deserialize<WebhookUpdateTrackingRequest>(body);
    if (evt?.Event == "TRACK_UPDATED")
    {
        Console.WriteLine($"Tracking update: {evt.Data?.TrackingUrl} {evt.Data?.TrackingNumber}");
    }
    return Results.Ok(new { status = "ok" });
});

app.Run();
