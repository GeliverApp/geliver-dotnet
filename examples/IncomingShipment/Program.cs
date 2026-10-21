using Geliver.Sdk;
using Geliver.Sdk.Models;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN");
var recipientAddressID = Environment.GetEnvironmentVariable("GELIVER_MERCHANT_ADDRESS_ID");
if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(recipientAddressID))
{
  Console.Error.WriteLine("Set GELIVER_TOKEN and GELIVER_MERCHANT_ADDRESS_ID.");
  return;
}

var client = new GeliverClient(token);
var shipment = await client.Shipments.CreateAsync(new CreateShipmentWithRecipientID
{
  IsReturn = true,
  Test = true,
  SenderAddress = new SenderAddressRequest
  {
    Name = "Müşteri Adı", Email = "customer@example.com", Phone = "+905051234567",
    Address1 = "Örnek Mahallesi 1", CountryCode = "TR", CityCode = "34", DistrictName = "Kadıköy", Zip = "34000",
  },
  RecipientAddressID = recipientAddressID,
  Length = "10", Width = "10", Height = "10", DistanceUnit = "cm", Weight = "1", MassUnit = "kg",
});

Console.WriteLine($"Gelecek gönderi: {shipment?.Id}");
