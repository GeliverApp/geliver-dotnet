using System.Text.Json;
using Geliver.Sdk;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN") ?? "YOUR_TOKEN";
var client = new GeliverClient(token);

var sender = await client.Addresses.CreateAsync(new {
  name = "ACME Inc.", email = "ops@acme.test", phone = "+905051234567",
  address1 = "Street 1", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
  districtName = "Esenyurt", districtID = 107605, zip = "34020", isRecipientAddress = false,
});

var shipment = await client.Shipments.CreateTestAsync(new {
  sourceCode = "API",
  senderAddressID = sender!["id"],
  recipientAddress = new {
    name = "John Doe", email = "john@example.com", address1 = "Dest St 2", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
    districtName = "Kadikoy", districtID = 100000, zip = "34000",
  },
  length = 10, width = 10, height = 10, distanceUnit = "cm",
  weight = 1, massUnit = "kg",
});

// Etiketler bazı akışlarda create sonrasında hazır olabilir; varsa hemen indirin
if (!string.IsNullOrEmpty(shipment!.LabelURL))
{
  var prePdf = await client.DownloadLabelForShipmentAsync(shipment!.Id);
  await File.WriteAllBytesAsync("label_pre.pdf", prePdf);
}
if (!string.IsNullOrEmpty(shipment!.ResponsiveLabelURL))
{
  var preHtml = await client.DownloadResponsiveLabelForShipmentAsync(shipment!.Id);
  await File.WriteAllTextAsync("label_pre.html", preHtml);
}

// Teklifler create yanıtında hazır olabilir; önce shipment.Offers'i kontrol edin
var offersObj = shipment!.Offers;
if (!(offersObj != null && (offersObj.PercentageCompleted >= 99 || offersObj.Cheapest != null)))
{
  while (true)
  {
    var sDict = await client.RequestAsync<Dictionary<string, object>>(HttpMethod.Get, $"/shipments/{shipment!.Id}");
    if (sDict!.TryGetValue("offers", out var offersRaw))
    {
      var offers = offersRaw as Dictionary<string, object>;
      if (offers != null && (Convert.ToInt32(offers["percentageCompleted"]) >= 99 || offers.ContainsKey("cheapest")))
      {
        var cheapest = offers["cheapest"] as Dictionary<string, object>;
        var tx = await client.Transactions.AcceptOfferAsync(cheapest!["id"].ToString()!);
        Console.WriteLine($"Transaction {tx!.Id}");
        if (tx.Shipment is not null) {
          Console.WriteLine($"Barcode: {tx.Shipment.Barcode}");
          Console.WriteLine($"Tracking number: {tx.Shipment.TrackingNumber}");
          Console.WriteLine($"Label URL: {tx.Shipment.LabelURL}");
          Console.WriteLine($"Tracking URL: {tx.Shipment.TrackingUrl}");
        }
        goto OffersAccepted;
      }
    }
    await Task.Delay(1000);
  }
}
else
{
  // create yanıtında hazırsa doğrudan kabul et (cheapest varsa)
  if (offersObj!.Cheapest != null)
  {
    var tx = await client.Transactions.AcceptOfferAsync(offersObj.Cheapest.Id!);
    Console.WriteLine($"Transaction {tx!.Id}");
    if (tx.Shipment is not null) {
      Console.WriteLine($"Barcode: {tx.Shipment.Barcode}");
      Console.WriteLine($"Tracking number: {tx.Shipment.TrackingNumber}");
      Console.WriteLine($"Label URL: {tx.Shipment.LabelURL}");
      Console.WriteLine($"Tracking URL: {tx.Shipment.TrackingUrl}");
    }
  }
}

OffersAccepted:

// Test modunda her GET /shipments isteği kargo durumunu bir adım ilerletir; prod'da webhook veya kendi kontrollerinizi kullanın
for (var i = 0; i < 5; i++) { await Task.Delay(1000); await client.Shipments.GetAsync(shipment!.Id); }
var latest = await client.Shipments.GetAsync(shipment!.Id);
Console.WriteLine($"Final tracking status: {latest!.TrackingStatus?.TrackingStatusCode} {latest!.TrackingStatus?.TrackingSubStatusCode}");

// Download labels
var pdf = await client.DownloadLabelForShipmentAsync(shipment!.Id);
await File.WriteAllBytesAsync("label.pdf", pdf);
var html = await client.DownloadResponsiveLabelForShipmentAsync(shipment!.Id);
await File.WriteAllTextAsync("label.html", html);
