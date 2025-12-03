namespace Geliver.Sdk.Models;

public class CreateAddressRequest
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }
    public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    public string CountryCode { get; set; } = null!;
    public string CityName { get; set; } = null!;
    public string CityCode { get; set; } = null!;
    public string DistrictName { get; set; } = null!;
    public int? DistrictID { get; set; }
    public string Zip { get; set; } = null!;
    public string? ShortName { get; set; }
    public bool? IsRecipientAddress { get; set; }
}

public class CreateShipmentRequestBase
{
    public string SenderAddressID { get; set; } = null!;
    public string? Length { get; set; }
    public string? Width { get; set; }
    public string? Height { get; set; }
    public string? DistanceUnit { get; set; }
    public string? Weight { get; set; }
    public string? MassUnit { get; set; }
    public string? ProviderServiceCode { get; set; }
    public string? ProviderAccountID { get; set; }
    public bool? ProductPaymentOnDelivery { get; set; }
    public bool? Test { get; set; }
    public OrderRequest? Order { get; set; }
}

public class CreateShipmentWithRecipientID : CreateShipmentRequestBase
{
    public string RecipientAddressID { get; set; } = null!;
}

public class CreateShipmentWithRecipientAddress : CreateShipmentRequestBase
{
    public RecipientAddressRequest RecipientAddress { get; set; } = null!;
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

public class RecipientAddressRequest
{
    public string Name { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Email { get; set; }
    public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    public string CountryCode { get; set; } = null!;
    public string CityName { get; set; } = null!;
    public string CityCode { get; set; } = null!;
    public string DistrictName { get; set; } = null!;
    public int? DistrictID { get; set; }
    public string? Zip { get; set; }
}

public class OrderRequest
{
    public string? SourceCode { get; set; }
    public string? SourceIdentifier { get; set; }
    public string OrderNumber { get; set; } = null!;
    public string? TotalAmount { get; set; }
    public string? TotalAmountCurrency { get; set; }
}
