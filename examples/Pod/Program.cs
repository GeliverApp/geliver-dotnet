using Geliver.Sdk;
using Geliver.Sdk.Models;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN");
if (string.IsNullOrEmpty(token)) { Console.Error.WriteLine("GELIVER_TOKEN required"); return; }
var client = new GeliverClient(token);

var sender = await client.Addresses.CreateSenderAsync(new {
  name = "POD Sender", email = "sender@example.com", phone = "+905000000098",
  address1 = "Hasan Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Esenyurt", zip = "34020"
});

var tx = await client.Transactions.CreateWithRecipientAddressAsync(new CreateShipmentWithRecipientAddress {
  SenderAddressID = Convert.ToString(sender!["id"]) ?? string.Empty,
  RecipientAddress = new RecipientAddressRequest {
    Name = "POD Recipient",
    Phone = "+905000000001",
    Address1 = "Atatürk Mahallesi",
    CountryCode = "TR",
    CityName = "Istanbul",
    CityCode = "34",
    DistrictName = "Esenyurt",
  },
  Length = "10.0", Width = "10.0", Height = "10.0", DistanceUnit = "cm",
  Weight = "1.0", MassUnit = "kg",
  ProviderServiceCode = "PTT_KAPIDA_ODEME",
  ProductPaymentOnDelivery = true,
  Order = new OrderRequest { OrderNumber = "POD-12345", TotalAmount = "150", TotalAmountCurrency = "TRY" },
});

Console.WriteLine($"transaction id: {tx?.Id}");
