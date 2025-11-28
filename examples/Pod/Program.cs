using Geliver.Sdk;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN");
if (string.IsNullOrEmpty(token)) { Console.Error.WriteLine("GELIVER_TOKEN required"); return; }
var client = new GeliverClient(token);

var sender = await client.Addresses.CreateSenderAsync(new {
  name = "POD Sender", email = "sender@example.com", phone = "+905000000098",
  address1 = "Hasan Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Esenyurt", zip = "34020"
});

var tx = await client.Transactions.CreateAsync(new {
  senderAddressID = sender!["id"],
  recipientAddress = new { name = "POD Recipient", phone = "+905000000001", address1 = "Atatürk Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Esenyurt" },
  length = "10.0", width = "10.0", height = "10.0", distanceUnit = "cm", weight = "1.0", massUnit = "kg",
  providerServiceCode = "PTT_KAPIDA_ODEME",
  productPaymentOnDelivery = true,
  order = new { orderNumber = "POD-12345", totalAmount = "150", totalAmountCurrency = "TRY" },
});

Console.WriteLine($"transaction id: {tx?.Id}");

