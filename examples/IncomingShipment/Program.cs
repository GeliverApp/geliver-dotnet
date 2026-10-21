using Geliver.Sdk;
using Geliver.Sdk.Models;

var token = Environment.GetEnvironmentVariable("GELIVER_TOKEN");
if (string.IsNullOrWhiteSpace(token))
{
  Console.Error.WriteLine("Set GELIVER_TOKEN.");
  return;
}

var client = new GeliverClient(token);
var shipment = await client.Shipments.CreateAsync(new CreateShipmentWithRecipientAddress
{
  IsReturn = true,
  Test = true,
  RecipientAddress = new RecipientAddressRequest
  {
    Name = "Müşteri Adı", Email = "customer@example.com", Phone = "+905051234567",
    Address1 = "Örnek Mahallesi 1", CountryCode = "TR", CityCode = "34", DistrictName = "Kadıköy", Zip = "34000",
  },
  Length = "10", Width = "10", Height = "10", DistanceUnit = "cm", Weight = "1", MassUnit = "kg",
});

Console.WriteLine($"Gelecek gönderi: {shipment?.Id}");
