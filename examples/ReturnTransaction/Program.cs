using Geliver.Sdk;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN");
var originalShipmentId = Environment.GetEnvironmentVariable("GELIVER_RETURN_SHIPMENT_ID") ?? args.FirstOrDefault();

if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(originalShipmentId))
{
  Console.WriteLine("Set GELIVER_TOKEN and GELIVER_RETURN_SHIPMENT_ID, or pass the shipment ID as the first argument.");
  return;
}

var client = new GeliverClient(token);
var tx = await client.Transactions.CreateReturnAsync(originalShipmentId, new { });
Console.WriteLine($"Transaction: {tx?.Id}");
Console.WriteLine($"Purchased return shipment: {tx?.Shipment?.Id}");
