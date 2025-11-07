namespace Geliver.Sdk.Models;

public class CreateAddressRequest
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public string? Phone { get; set; }
    public required string Address1 { get; set; }
    public string? Address2 { get; set; }
    public required string CountryCode { get; set; }
    public required string CityName { get; set; }
    public required string CityCode { get; set; }
    public required string DistrictName { get; set; }
    public required int DistrictID { get; set; }
    public required string Zip { get; set; }
    public string? ShortName { get; set; }
    public bool? IsRecipientAddress { get; set; }
}

public class CreateShipmentRequestBase
{
    public required string SourceCode { get; set; }
    public required string SenderAddressID { get; set; }
    public string? Length { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? DistanceUnit { get; set; }
    public string? Weight { get; set; }
    public string? MassUnit { get; set; }
    public string? ProviderServiceCode { get; set; }
}

public class CreateShipmentWithRecipientID : CreateShipmentRequestBase
{
    public required string RecipientAddressID { get; set; }
}

public class CreateShipmentWithRecipientAddress : CreateShipmentRequestBase
{
    public required Dictionary<string, object> RecipientAddress { get; set; }
}

public class UpdatePackageRequest
{
    public string? Height { get; set; }
    public string? Width { get; set; }
    public string? Length { get; set; }
    public string? DistanceUnit { get; set; }
    public string? Weight { get; set; }
    public string? MassUnit { get; set; }
}
