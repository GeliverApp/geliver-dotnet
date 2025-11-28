using Geliver.Sdk;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN");
if (string.IsNullOrEmpty(token)) { Console.Error.WriteLine("GELIVER_TOKEN required"); return; }
var client = new GeliverClient(token);

var sender = await client.Addresses.CreateSenderAsync(new {
  name = "OwnAg Sender", email = "sender@example.com", phone = "+905000000097",
  address1 = "Hasan Mahallesi", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Esenyurt", zip = "34020"
});

var tx = await client.Transactions.CreateAsync(new {
  senderAddressID = sender!["id"],
  recipientAddress = new { name = "OwnAg Recipient", phone = "+905000000002", address1 = "Dest 2", countryCode = "TR", cityName = "Istanbul", cityCode = "34", districtName = "Esenyurt" },
  length = "10.0", width = "10.0", height = "10.0", distanceUnit = "cm", weight = "1.0", massUnit = "kg",
  providerServiceCode = "SURAT_STANDART",
  providerAccountID = "c0dfdb42-012d-438c-9d49-98d13b4d4a2b",
});

Console.WriteLine($"transaction id: {tx?.Id}");

