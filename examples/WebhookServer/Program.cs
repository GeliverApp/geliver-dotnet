var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/webhooks/geliver", async (HttpRequest req) => {
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    // TODO: verify signature (disabled for now)
    return Results.Ok(new { status = "ok" });
});

app.Run();

