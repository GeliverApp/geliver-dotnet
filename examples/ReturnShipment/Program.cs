using Geliver.Sdk;
using Geliver.Sdk.Models;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN");
var originalShipmentId = Environment.GetEnvironmentVariable("GELIVER_RETURN_SHIPMENT_ID") ?? args.FirstOrDefault();

if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(originalShipmentId))
{
  Console.WriteLine("Set GELIVER_TOKEN and GELIVER_RETURN_SHIPMENT_ID, or pass the shipment ID as the first argument.");
  return;
}

var client = new GeliverClient(token);
var returned = await client.Shipments.CreateReturnAsync(originalShipmentId, new { });
if (returned?.Id is null)
{
  Console.WriteLine("Return shipment was not created.");
  return;
}

var returnShipmentId = returned.Id;
Console.WriteLine($"Return shipment: {returnShipmentId}");
Console.WriteLine("Label is not purchased yet. This example waits for offers and buys it with AcceptOffer.");

Shipment current = returned;
var deadline = DateTime.UtcNow.AddSeconds(60);

while (current.Offers?.Cheapest?.Id is null)
{
  if (DateTime.UtcNow >= deadline) throw new TimeoutException("Timed out waiting for return offers.");
  Console.WriteLine($"Waiting offers... {current.Offers?.PercentageCompleted ?? 0}%");
  await Task.Delay(TimeSpan.FromSeconds(1));
  current = await client.Shipments.GetAsync(returnShipmentId) ?? throw new InvalidOperationException("Return shipment not found.");
}

var tx = await client.Transactions.AcceptOfferAsync(current.Offers!.Cheapest!.Id!);
Console.WriteLine($"Transaction: {tx?.Id}");
Console.WriteLine($"Purchased return shipment: {tx?.Shipment?.Id}");
