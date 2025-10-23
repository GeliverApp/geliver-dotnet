# Geliver C# SDK (.NET 6+)

Geliver C# SDK — official .NET client for Geliver Kargo Pazaryeri (Shipping Marketplace) API.
Türkiye’nin e‑ticaret gönderim altyapısı için kolay kargo entegrasyonu sağlar.

• Dokümantasyon (TR/EN): https://docs.geliver.io

## İçindekiler

- Kurulum
- Hızlı Başlangıç
- Adım Adım
- Webhooklar
- Testler
- Modeller
- Enum Kullanımı
- Notlar ve İpuçları

## Kurulum

- Yerelde derleme: `dotnet build sdks/csharp/src/Geliver.Sdk`

## Hızlı Başlangıç

```csharp
using Geliver.Sdk;

var client = new GeliverClient(token: "YOUR_TOKEN");
var sender = await client.Addresses.CreateSenderAsync(new { name = "ACME Inc.", email = "ops@acme.test", address1 = "Street 1", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Esenyurt", districtID = 107605, zip = "34020" });
var shipment = await client.Shipments.CreateTestAsync(new {
  sourceCode = "API",
  senderAddressID = sender!["id"],
  recipientAddress = new { name = "John Doe", email = "john@example.com", address1 = "Dest St 2", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Kadikoy", districtID = 100000, zip = "34000" },
  length = 10, width = 10, height = 10, distanceUnit = "cm", weight = 1, massUnit = "kg",
});
```

## Akış (TR)

1. Geliver Kargo API tokenı alın (https://app.geliver.io/apitokens adresinden)
2. Gönderici adresi oluşturun (Addresses.CreateSenderAsync)
3. Gönderiyi alıcıyı ID ile ya da adres nesnesi ile vererek oluşturun (Shipments.CreateAsync)
4. Teklifleri bekleyip kabul edin (Transactions.AcceptOfferAsync)
5. Barkod, takip numarası, etiket URL’leri Transaction içindeki Shipment’ten okunur
6. Test gönderilerinde her GET isteği kargo durumunu bir adım ilerletir; prod'da webhook kurun
7. Etiketleri indirin (DownloadLabelForShipmentAsync, DownloadResponsiveLabelForShipmentAsync)
8. İade gönderisi gerekiyorsa Shipments.CreateReturnAsync kullanın

```csharp
using Geliver.Sdk;

var client = new GeliverClient(token: "YOUR_TOKEN");

// 1) Create sender address
var sender = await client.Addresses.CreateSenderAsync(new {
  name = "ACME Inc.", email = "ops@acme.test", phone = "+905051234567",
  address1 = "Street 1", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
  districtName = "Esenyurt", districtID = 107605, zip = "34020",
});

// 2) Create shipment (Option A: inline recipient address)
var shipment = await client.Shipments.CreateAsync(new {
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
if (!string.IsNullOrEmpty(shipment!.LabelURL)) {
  var prePdf = await client.DownloadLabelForShipmentAsync(shipment!.Id);
  await File.WriteAllBytesAsync("label_pre.pdf", prePdf);
}
if (!string.IsNullOrEmpty(shipment!.ResponsiveLabelURL)) {
  var preHtml = await client.DownloadResponsiveLabelForShipmentAsync(shipment!.Id);
  await File.WriteAllTextAsync("label_pre.html", preHtml);
}

Dictionary<string, object>? s;
while (true)
{
  s = await client.RequestAsync<Dictionary<string, object>>(HttpMethod.Get, $"/shipments/{shipment!.Id}");
  var offers = s!["offers"] as Dictionary<string, object>;
  if (offers != null && (Convert.ToInt32(offers["percentageCompleted"]) >= 99 || offers.ContainsKey("cheapest"))) {
    var cheapest = offers["cheapest"] as Dictionary<string, object>;
    var tx = await client.Transactions.AcceptOfferAsync(cheapest!["id"].ToString()!);
    if (tx.Shipment is not null) {
      Console.WriteLine($"Barcode: {tx.Shipment.Barcode}");
      Console.WriteLine($"Tracking number: {tx.Shipment.TrackingNumber}");
      Console.WriteLine($"Label URL: {tx.Shipment.LabelURL}");
      Console.WriteLine($"Tracking URL: {tx.Shipment.TrackingUrl}");
    }
    break;
  }
  await Task.Delay(1000);
}

// Test mode only: trigger 5 lightweight GET calls to advance status; in prod use webhooks
for (var i = 0; i < 5; i++) { await client.Shipments.GetAsync(shipment!.Id); await Task.Delay(1000); }
var final = await client.Shipments.GetAsync(shipment!.Id);
Console.WriteLine($"Final tracking status: {final!.TrackingStatus?.TrackingStatusCode} {final!.TrackingStatus?.TrackingSubStatusCode}");
```

## Alıcı ID'si ile oluşturma (recipientAddressID)

```csharp
// Önce alıcı adresini oluşturun ve ID alın
var recipient = await client.Addresses.CreateRecipientAsync(new {
  name = "John Doe", email = "john@example.com", address1 = "Dest St 2", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
  districtName = "Kadikoy", districtID = 100000, zip = "34000",
});

// Ardından recipientAddressID ile gönderi oluşturun
var createdDirect = await client.Shipments.CreateAsync(new {
  sourceCode = "API",
  senderAddressID = sender!["id"],
  recipientAddressID = recipient!["id"],
  providerServiceCode = "MNG_STANDART",
  length = 10, width = 10, height = 10, distanceUnit = "cm",
  weight = 1, massUnit = "kg",
});
```

---

## Webhooklar

- `/webhooks/geliver` için bir controller veya minimal API endpoint'i tanımlayın; doğrulama için `Webhooks.Verify(body, headers, enableVerification: false)` kullanabilirsiniz.

## Testler

- Özel bir `HttpMessageHandler` enjekte ederek test yazabilirsiniz.
- Üretilmiş model sınıfları `Geliver.Sdk.Models` altında bulunur (OpenAPI’den otomatik).

Manuel takip kontrolü (isteğe bağlı)

```csharp
var latest = await client.Shipments.GetAsync(created!.Id);
var ts = latest is not null ? (latest as Dictionary<string, object>) : null; // or create a typed wrapper
// If you prefer dynamic, adjust your deserialization to a DTO with TrackingStatus

// Download labels
var pdf = await client.DownloadLabelForShipmentAsync(shipment!.Id);
await File.WriteAllBytesAsync("label.pdf", pdf);
var html = await client.DownloadResponsiveLabelForShipmentAsync(shipment!.Id);
await File.WriteAllTextAsync("label.html", html);
```

## Modeller

- Shipment, Transaction, TrackingStatus, Address, ParcelTemplate, ProviderAccount, Webhook, Offer, PriceQuote ve daha fazlası.
- Tam liste: `Geliver.Sdk.Models`.

## Enum Kullanımı (TR)

```csharp
using Geliver.Sdk.Models;

var s = await client.Shipments.GetAsync(shipment!.Id);
if (s!.LabelFileType == ShipmentLabelFileType.PDF) {
  Console.WriteLine("PDF etiket hazır");
}
```

## Notlar ve İpuçları (TR)

- Sayısal alanlar `decimal` olarak işlenir ve JSON string sayılar desteklenir.
- Teklif beklerken 1 sn aralıkla tekrar sorgulayın.
- Test gönderisi için `new { ..., test = true }` veya `CreateTestAsync(...)` kullanın.
- İlçe seçimi: districtID (number) kullanınız. districtName her zaman doğru eşleşmeyebilir.
- Şehir/İlçe seçimi: cityCode ve cityName birlikte/ayrı gönderilebilir; cityCode daha güvenlidir. Şehir/ilçe listeleri için API:

```csharp
var cities = await client.Geo.ListCitiesAsync("TR");
var districts = await client.Geo.ListDistrictsAsync("TR", "34");
```

---

[![Geliver Kargo Pazaryeri](https://geliver.io/geliverlogo.png)](https://geliver.io/)
Geliver Kargo Pazaryeri: https://geliver.io/

Etiketler (Tags): csharp, dotnet, sdk, api-client, geliver, kargo, kargo-pazaryeri, shipping, e-commerce, turkey
