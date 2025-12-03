# Geliver C# SDK (.NET 6+)

[![NuGet](https://img.shields.io/nuget/v/Geliver.Sdk.svg)](https://www.nuget.org/packages/Geliver.Sdk)

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

NuGet üzerinden yükleyin:

```bash
dotnet add package Geliver.Sdk
```

veya Package Manager Console'da:

```
Install-Package Geliver.Sdk
```

Alternatif olarak, .csproj dosyanıza manuel ekleyin:

```xml
<PackageReference Include="Geliver.Sdk" Version="*" />
```

Yerelde derleme için: `dotnet build src/Geliver.Sdk`

## Hızlı Başlangıç

```csharp
using Geliver.Sdk;

var client = new GeliverClient(token: "YOUR_TOKEN");
var sender = await client.Addresses.CreateSenderAsync(new { name = "ACME Inc.", email = "ops@acme.test", phone = "+905051234567", address1 = "Hasan Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Esenyurt", zip = "34020" });
var shipment = await client.Shipments.CreateTestAsync(new {
  sourceCode = "API",
  senderAddressID = sender!["id"],
  recipientAddress = new { name = "John Doe", email = "john@example.com", phone = "+905051234568", address1 = "Atatürk Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Kadıköy", zip = "34000" },
  length = 10, width = 10, height = 10, distanceUnit = "cm", weight = 1, massUnit = "kg",
  order = new {
    orderNumber = "WEB-12345",
    // sourceIdentifier alanına mağazanızın tam adresini (ör. https://magazam.com) ekleyin.
    sourceIdentifier = "https://magazam.com",
    totalAmount = "150",
    totalAmountCurrency = "TRY",
  },
});
```

Canlı ortamda `CreateTestAsync(...)` yerine `Shipments.CreateAsync(...)` çağırın.

## Akış (TR)

1. Geliver Kargo API tokenı alın (https://app.geliver.io/apitokens adresinden)
2. Gönderici adresi oluşturun (Addresses.CreateSenderAsync)
3. Gönderiyi alıcıyı ID ile ya da adres nesnesi ile vererek oluşturun (Shipments.CreateAsync)
4. Teklifleri bekleyip kabul edin (Transactions.AcceptOfferAsync)
5. Barkod, takip numarası, etiket URL’leri Transaction içindeki Shipment’ten okunur
6. Test gönderilerinde her GET isteği kargo durumunu bir adım ilerletir; prod'da webhook kurun
7. Etiketleri indirin (Transaction/Shipment içindeki URL'den indirin)
8. İade gönderisi gerekiyorsa Shipments.CreateReturnAsync kullanın

```csharp
using Geliver.Sdk;

var client = new GeliverClient(token: "YOUR_TOKEN");

// 1) Gönderici adresi oluşturun. Her gönderici adresi için tek seferlik kullanılır. Gönderici ID'sini saklayınız.
var sender = await client.Addresses.CreateSenderAsync(new {
  name = "ACME Inc.", email = "ops@acme.test", phone = "+905051234567",
  address1 = "Hasan Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
  districtName = "Esenyurt", zip = "34020",
});

// 2) Alıcı adres bilgileri ile Gönderi oluşturun
var shipment = await client.Shipments.CreateAsync(new {
  sourceCode = "API",
  senderAddressID = sender!["id"],
  recipientAddress = new {
    name = "John Doe", email = "john@example.com", address1 = "Atatürk Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
    districtName = "Kadıköy", zip = "34000",
  },
  length = 10, width = 10, height = 10, distanceUnit = "cm",
  weight = 1, massUnit = "kg",
  order = new {
    orderNumber = "WEB-12345",
    // sourceIdentifier alanına mağazanızın tam adresini (ör. https://magazam.com) ekleyin.
    sourceIdentifier = "https://magazam.com",
    totalAmount = "150",
    totalAmountCurrency = "TRY",
  },
});

// Etiketler bazı akışlarda create sonrasında hazır olabilir; varsa hemen indirin
if (!string.IsNullOrEmpty(shipment!.LabelURL)) {
  // Ek GET yapmadan doğrudan URL'den indirin
  var prePdf = await client.DownloadLabelAsync(shipment!.LabelURL);
  await File.WriteAllBytesAsync("label_pre.pdf", prePdf);
}
if (!string.IsNullOrEmpty(shipment!.ResponsiveLabelURL)) {
  var preHtml = await client.DownloadResponsiveLabelAsync(shipment!.ResponsiveLabelURL);
  await File.WriteAllTextAsync("label_pre.html", preHtml);
}

var offers = shipment!.Offers;
if (offers?.Cheapest is null)
{
  throw new InvalidOperationException("Teklifler henüz hazır değil; GET /shipments ile tekrar kontrol edin.");
}

var tx = await client.Transactions.AcceptOfferAsync(offers.Cheapest.Id!);
if (tx.Shipment is not null) {
  Console.WriteLine($"Barcode: {tx.Shipment.Barcode}");
  Console.WriteLine($"Tracking number: {tx.Shipment.TrackingNumber}");
  Console.WriteLine($"Label URL: {tx.Shipment.LabelURL}");
  Console.WriteLine($"Tracking URL: {tx.Shipment.TrackingUrl}");
}

// Sadece TEST için: get yaparak gönderi durumu her istekte güncellenir; Prodda webhook veya periyodik tetikleme gerekir.

for (var i = 0; i < 5; i++) { await client.Shipments.GetAsync(shipment!.Id); await Task.Delay(1000); }
var final = await client.Shipments.GetAsync(shipment!.Id);
Console.WriteLine($"Final tracking status: {final!.TrackingStatus?.TrackingStatusCode} {final!.TrackingStatus?.TrackingSubStatusCode}");
```

## Alıcı ID'si ile oluşturma (recipientAddressID)

```csharp
// Önce alıcı adresini oluşturun ve ID alın
var recipient = await client.Addresses.CreateRecipientAsync(new {
  name = "John Doe", email = "john@example.com", address1 = "Atatürk Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34",
  districtName = "Kadıköy", zip = "34000",
});

// Ardından recipientAddressID ile gönderi oluşturun
// Canlı ortamda CreateTestAsync yerine CreateAsync kullanın.
var createdDirect = await client.Shipments.CreateTestAsync(new {
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

```csharp
using System.Linq;
using System.Text.Json;
using Geliver.Sdk;
using Geliver.Sdk.Models;

app.MapPost("/webhooks/geliver", async (HttpRequest req) =>
{
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync();
    if (!Webhooks.Verify(body, req.Headers.ToDictionary(k => k.Key, v => string.Join(",", v.Value)), enableVerification: false))
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
```

## Testler

- Özel bir `HttpMessageHandler` enjekte ederek test yazabilirsiniz.
- Üretilmiş model sınıfları `Geliver.Sdk.Models` altında bulunur (OpenAPI’den otomatik).

Manuel takip kontrolü (isteğe bağlı)

```csharp
var latest = await client.Shipments.GetAsync(created!.Id);
var ts = latest is not null ? (latest as Dictionary<string, object>) : null; // or create a typed wrapper
// If you prefer dynamic, adjust your deserialization to a DTO with TrackingStatus

// Download labels
// Teklif kabulünden sonra Transaction.Shipment içindeki URL'leri kullanın (ek GET yok)
var pdf = await client.DownloadLabelAsync(tx!.Shipment!.LabelURL!);
await File.WriteAllBytesAsync("label.pdf", pdf);
var html = await client.DownloadResponsiveLabelAsync(tx!.Shipment!.ResponsiveLabelURL!);
await File.WriteAllTextAsync("label.html", html);
```

### Gönderi Listeleme, Getir, Güncelle, İptal, Klonla

- Listeleme (docs): https://docs.geliver.io/docs/shipments_and_transaction/list_shipments
- Gönderi getir (docs): https://docs.geliver.io/docs/shipments_and_transaction/list_shipments
- Paket güncelle (docs): https://docs.geliver.io/docs/shipments_and_transaction/update_package_shipment
- Gönderi iptal (docs): https://docs.geliver.io/docs/shipments_and_transaction/cancel_shipment
- Gönderi klonla (docs): https://docs.geliver.io/docs/shipments_and_transaction/clone_shipment

```csharp
// Listeleme (sayfalandırma)
var page = await client.Shipments.ListWithEnvelopeAsync(new { page = 1, limit = 20 });
foreach (var shipment in page?.Data ?? new List<Geliver.Sdk.Models.Shipment>())
{
    Console.WriteLine($"{shipment.Id} {shipment.StatusCode}");
}

// Getir
var fetched = await client.Shipments.GetAsync("SHIPMENT_ID");
Console.WriteLine($"Tracking: {fetched?.TrackingStatus?.TrackingStatusCode} {fetched?.TrackingStatus?.TrackingSubStatusCode}");

// Paket güncelle (eni, boyu, yüksekliği ve ağırlığı string gönderin)
await client.Shipments.UpdatePackageAsync(fetched!.Id!, new {
    length = "12.0",
    width = "12.0",
    height = "10.0",
    distanceUnit = "cm",
    weight = "1.2",
    massUnit = "kg",
});

// İptal
await client.Shipments.CancelAsync(fetched.Id!);

// Klonla
var cloned = await client.Shipments.CloneAsync(fetched.Id!);
Console.WriteLine($"Cloned shipment: {cloned?.Id}");
```

---

## İade Gönderisi Oluşturun

```csharp
var returned = await client.Shipments.CreateReturnAsync(shipment!.Id, new {
  willAccept = true,
  providerServiceCode = "SURAT_STANDART",
  count = 1,
});
```

Not:

- `providerServiceCode` alanı opsiyoneldir. Varsayılan olarak orijinal gönderinin sağlayıcısı kullanılır; isterseniz bu alanı vererek değiştirebilirsiniz.
- `senderAddress` alanı opsiyoneldir. Varsayılan olarak orijinal gönderinin alıcı adresi kullanılır; isterseniz bu alanı vererek değiştirebilirsiniz.

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
- Test gönderisi için `new { ..., test = true }` veya `CreateTestAsync(...)` kullanın; canlı ortamda `Shipments.CreateAsync(...)` tercih edin.

- Takip numarası ile takip URL'si bazı kargo firmalarında teklif kabulünün hemen ardından oluşmayabilir. Paketi kargo şubesine teslim ettiğinizde veya kargo sizden teslim aldığında bu alanlar tamamlanır. Webhooklar ile değerleri otomatik çekebilir ya da teslimden sonra `shipment` GET isteği yaparak güncel bilgileri alabilirsiniz.
- Şehir/İlçe seçimi: cityCode ve cityName birlikte/ayrı gönderilebilir; cityCode daha güvenlidir. Şehir/ilçe listeleri için API:

```csharp
var cities = await client.Geo.ListCitiesAsync("TR");
var districts = await client.Geo.ListDistrictsAsync("TR", "34");
```

- Adres kuralları: phone alanı hem gönderici hem alıcı adresleri için zorunludur. Zip alanı gönderici adresi için zorunludur; alıcı adresi için opsiyoneldir. `Addresses.CreateSenderAsync(...)` phone/zip eksikse, `Addresses.CreateRecipientAsync(...)` phone eksikse hata verir.

## Hatalar ve İstisnalar

- İstemci şu durumlarda `GeliverApiException` fırlatır: (1) HTTP 4xx/5xx; (2) JSON envelope `result == false`.
- Hata alanları: `Code`, `AdditionalMessage`, `Status`, `Body`, `Message`.

```csharp
try
{
    await client.Shipments.CreateAsync(new { /* ... */ });
}
catch (GeliverApiException ex)
{
    Console.WriteLine($"code: {ex.Code}");
    Console.WriteLine($"message: {ex.Message}");
    Console.WriteLine($"additional: {ex.AdditionalMessage}");
    Console.WriteLine($"status: {ex.Status}");
}
```

## Örnekler

- Tam akış: `examples/FullFlow/Program.cs`
- Tek aşamada gönderi (Create Transaction): örnek kod

```csharp
var tx = await client.Transactions.CreateWithRecipientAddressAsync(new CreateShipmentWithRecipientAddress {
  SenderAddressID = Convert.ToString(sender!["id"]) ?? string.Empty,
  RecipientAddress = new RecipientAddressRequest { Name = "OneStep Recipient", Phone = "+905000000000", Address1 = "Atatürk Mahallesi", CountryCode = "TR", CityName = "Istanbul", CityCode = "34", DistrictName = "Esenyurt" },
  Length = "10.0", Width = "10.0", Height = "10.0", DistanceUnit = "cm", Weight = "1.0", MassUnit = "kg",
});
```

- Kapıda ödeme: örnek kod

```csharp
var txPod = await client.Transactions.CreateWithRecipientAddressAsync(new CreateShipmentWithRecipientAddress {
  SenderAddressID = Convert.ToString(sender!["id"]) ?? string.Empty,
  RecipientAddress = new RecipientAddressRequest { Name = "POD Recipient", Phone = "+905000000001", Address1 = "Atatürk Mahallesi", CountryCode = "TR", CityName = "Istanbul", CityCode = "34", DistrictName = "Esenyurt" },
  Length = "10.0", Width = "10.0", Height = "10.0", DistanceUnit = "cm", Weight = "1.0", MassUnit = "kg",
  ProviderServiceCode = "PTT_KAPIDA_ODEME",
  ProductPaymentOnDelivery = true,
  Order = new OrderRequest { OrderNumber = "POD-12345", TotalAmount = "150", TotalAmountCurrency = "TRY" },
});
```

- Kendi anlaşmanızla etiket satın alma: örnek kod

```csharp
var txOwn = await client.Transactions.CreateWithRecipientAddressAsync(new CreateShipmentWithRecipientAddress {
  SenderAddressID = Convert.ToString(sender!["id"]) ?? string.Empty,
  RecipientAddress = new RecipientAddressRequest { Name = "OwnAg Recipient", Phone = "+905000000002", Address1 = "Atatürk Mahallesi", CountryCode = "TR", CityName = "Istanbul", CityCode = "34", DistrictName = "Esenyurt" },
  Length = "10.0", Width = "10.0", Height = "10.0", DistanceUnit = "cm", Weight = "1.0", MassUnit = "kg",
  ProviderServiceCode = "SURAT_STANDART",
  ProviderAccountID = "c0dfdb42-012d-438c-9d49-98d13b4d4a2b",
});
```

---

[![Geliver Kargo Pazaryeri](https://geliver.io/geliverlogo.png)](https://geliver.io/)
Geliver Kargo Pazaryeri: https://geliver.io/

Etiketler (Tags): csharp, dotnet, sdk, api-client, geliver, kargo, kargo-pazaryeri, shipping, e-commerce, turkey
