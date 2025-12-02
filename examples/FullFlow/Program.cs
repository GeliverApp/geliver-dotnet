using System.Text.Json;
using Geliver.Sdk;
using Geliver.Sdk.Models;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN") ?? "YOUR_TOKEN";
var client = new GeliverClient(token);

var sender = await client.Addresses.CreateAsync(new {
  name = "ACME Inc.", email = "ops@acme.test", phone = "+905051234567",
  address1 = "Hasan Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
  districtName = "Esenyurt", zip = "34020", isRecipientAddress = false,
});

var shipment = await client.Shipments.CreateTestAsync(new {
  senderAddressID = sender!["id"],
  recipientAddress = new {
    name = "John Doe", email = "john@example.com", address1 = "Atatürk Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
    districtName = "Kadıköy", zip = "34000",
  },
  order = new { orderNumber = "ABC12333322", sourceIdentifier = "https://magazaadresiniz.com", totalAmount = "150", totalAmountCurrency = "TRY" },
  // Request dimensions/weight must be strings
  length = "10.0", width = "10.0", height = "10.0", distanceUnit = "cm",
  weight = "1.0", massUnit = "kg",
});

// Etiket indirme: Teklif kabulünden sonra (Transaction) gelen URL'leri kullanabilirsiniz de; URL'lere her shipment nesnesinin içinden ulaşılır.

// Teklifler create yanıtındaki offers alanında gelir
var typedOffers = shipment!.Offers;
if (typedOffers?.Cheapest is null)
{
  Console.WriteLine("Error: No cheapest offer available (henüz hazır değil)");
  return;
}

Transaction? tx;
try
{
  tx = await client.Transactions.AcceptOfferAsync(typedOffers.Cheapest.Id!);
}
catch (Exception ex)
{
  Console.WriteLine($"Accept offer error: {ex.Message}");
  if (ex is HttpRequestException httpEx && httpEx.Data.Contains("ResponseBody"))
  {
    Console.WriteLine($"API Error: {httpEx.Data["ResponseBody"]}");
  }
  return;
}
Console.WriteLine($"Transaction {tx!.Id}");
if (tx.Shipment is not null) {
  Console.WriteLine($"Barcode: {tx.Shipment.Barcode}");
  Console.WriteLine($"Tracking number: {tx.Shipment.TrackingNumber}");
  Console.WriteLine($"Label URL: {tx.Shipment.LabelURL}");
  Console.WriteLine($"Tracking URL: {tx.Shipment.TrackingUrl}");

  // Etiket indirme: LabelFileType kontrolü
  // Eğer LabelFileType "PROVIDER_PDF" ise, LabelURL'den indirilen PDF etiket kullanılmalıdır.
  // Eğer LabelFileType "PDF" ise, responsiveLabelURL (HTML) dosyası kullanılabilir.
  if (tx.Shipment.LabelFileType == "PROVIDER_PDF")
  {
    // PROVIDER_PDF: Sadece PDF etiket kullanılmalı
    if (!string.IsNullOrEmpty(tx.Shipment.LabelURL))
    {
      var pdf2 = await client.DownloadLabelAsync(tx.Shipment.LabelURL);
      await File.WriteAllBytesAsync("label.pdf", pdf2);
      Console.WriteLine("PDF etiket indirildi (PROVIDER_PDF)");
    }
  }
  else if (tx.Shipment.LabelFileType == "PDF")
  {
    // PDF: ResponsiveLabel (HTML) kullanılabilir
    if (!string.IsNullOrEmpty(tx.Shipment.ResponsiveLabelURL))
    {
      var html2 = await client.DownloadResponsiveLabelAsync(tx.Shipment.ResponsiveLabelURL);
      await File.WriteAllTextAsync("label.html", html2);
      Console.WriteLine("HTML etiket indirildi (PDF)");
    }
    // İsteğe bağlı olarak PDF de indirilebilir
    if (!string.IsNullOrEmpty(tx.Shipment.LabelURL))
    {
      var pdf2 = await client.DownloadLabelAsync(tx.Shipment.LabelURL);
      await File.WriteAllBytesAsync("label.pdf", pdf2);
    }
  }
}

// Test modunda her GET /shipments isteği kargo durumunu bir adım ilerletir; prod'da webhook veya kendi kontrollerinizi kullanın
for (var i = 0; i < 5; i++) { await Task.Delay(1000); await client.Shipments.GetAsync(shipment!.Id); }
var latest = await client.Shipments.GetAsync(shipment!.Id);
Console.WriteLine($"Final tracking status: {latest!.TrackingStatus?.TrackingStatusCode} {latest!.TrackingStatus?.TrackingSubStatusCode}");

// Download labels
var pdf = await client.DownloadLabelForShipmentAsync(shipment!.Id);
await File.WriteAllBytesAsync("label.pdf", pdf);
var html = await client.DownloadResponsiveLabelForShipmentAsync(shipment!.Id);
await File.WriteAllTextAsync("label.html", html);
