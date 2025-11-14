// Auto-generated from openapi.yaml
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Geliver.Sdk.Models;

/// <summary>Address model</summary>
public class Address {
  public string? Address1 { get; set; }
  public string? Address2 { get; set; }
  public City? City { get; set; }
  public string? CityCode { get; set; }
  public string? CityName { get; set; }
  public string? CountryCode { get; set; }
  public string? CountryName { get; set; }
  public string? CreatedAt { get; set; }
  public District? District { get; set; }
  public int? DistrictID { get; set; }
  public string? DistrictName { get; set; }
  public string? Email { get; set; }
  public string? Id { get; set; }
  public bool? IsActive { get; set; }
  public bool? IsDefaultReturnAddress { get; set; }
  public bool? IsDefaultSenderAddress { get; set; }
  public bool? IsInvoiceAddress { get; set; }
  public bool? IsRecipientAddress { get; set; }
  public JSONContent? Metadata { get; set; }
  public string? Name { get; set; }
  public string? Owner { get; set; }
  public string? Phone { get; set; }
  public string? ShortName { get; set; }
  public string? Source { get; set; }
  public string? State { get; set; }
  public string? StreetID { get; set; }
  public string? StreetName { get; set; }
  public bool? Test { get; set; }
  public string? UpdatedAt { get; set; }
  public string? Zip { get; set; }
}

/// <summary>City model</summary>
public class City {
  public string? AreaCode { get; set; }
  public string? CityCode { get; set; }
  public string? CountryCode { get; set; }
  /// <summary>Model</summary>
  public string? Name { get; set; }
}

/// <summary>DbStringArray model</summary>
public class DbStringArray {
}

/// <summary>District model</summary>
public class District {
  public string? CityCode { get; set; }
  public string? CountryCode { get; set; }
  public int? DistrictID { get; set; }
  /// <summary>Model</summary>
  public string? Name { get; set; }
  public string? RegionCode { get; set; }
}

// Duration model removed; API returns integer timestamp for duration fields.

/// <summary>Item model</summary>
public class Item {
  public string? CountryOfOrigin { get; set; }
  public string? CreatedAt { get; set; }
  public string? Currency { get; set; }
  public string? CurrencyLocal { get; set; }
  public string? Id { get; set; }
  public string? MassUnit { get; set; }
  public string? MaxDeliveryTime { get; set; }
  public string? MaxShipTime { get; set; }
  public string? Owner { get; set; }
  public int? Quantity { get; set; }
  public string? Sku { get; set; }
  public bool? Test { get; set; }
  public string? Title { get; set; }
  public string? TotalPrice { get; set; }
  public string? TotalPriceLocal { get; set; }
  public string? UnitPrice { get; set; }
  public string? UnitPriceLocal { get; set; }
  public string? UnitWeight { get; set; }
  public string? UpdatedAt { get; set; }
  public string? VariantTitle { get; set; }
}

/// <summary>JSONContent model</summary>
public class JSONContent {
}

/// <summary>Offer model</summary>
public class Offer {
  public string? Amount { get; set; }
  public string? AmountLocal { get; set; }
  public string? AmountLocalOld { get; set; }
  public string? AmountLocalTax { get; set; }
  public string? AmountLocalVat { get; set; }
  public string? AmountOld { get; set; }
  public string? AmountTax { get; set; }
  public string? AmountVat { get; set; }
  public long? AverageEstimatedTime { get; set; }
  public string? AverageEstimatedTimeHumanReadible { get; set; }
  public string? BonusBalance { get; set; }
  public string? CreatedAt { get; set; }
  public string? Currency { get; set; }
  public string? CurrencyLocal { get; set; }
  public string? DiscountRate { get; set; }
  public string? DurationTerms { get; set; }
  public string? EstimatedArrivalTime { get; set; }
  public string? Id { get; set; }
  public string? IntegrationType { get; set; }
  public bool? IsAccepted { get; set; }
  public bool? IsC2C { get; set; }
  public bool? IsGlobal { get; set; }
  public bool? IsMainOffer { get; set; }
  public bool? IsProviderAccountOffer { get; set; }
  public long? MaxEstimatedTime { get; set; }
  public long? MinEstimatedTime { get; set; }
  public string? Owner { get; set; }
  public decimal? PredictedDeliveryTime { get; set; }
  public string? ProviderAccountID { get; set; }
  public string? ProviderAccountName { get; set; }
  public string? ProviderAccountOwnerType { get; set; }
  public string? ProviderCode { get; set; }
  public string? ProviderServiceCode { get; set; }
  public string? ProviderTotalAmount { get; set; }
  public decimal? Rating { get; set; }
  public string? ScheduleDate { get; set; }
  public string? ShipmentTime { get; set; }
  public bool? Test { get; set; }
  public string? TotalAmount { get; set; }
  public string? TotalAmountLocal { get; set; }
  public string? UpdatedAt { get; set; }
}

/// <summary>OfferList model</summary>
public class OfferList {
  public bool? AllowOfferFallback { get; set; }
  public Offer? Cheapest { get; set; }
  public string? CreatedAt { get; set; }
  public Offer? Fastest { get; set; }
  public string? Height { get; set; }
  public List<string> ItemIDs { get; set; }
  public string? Length { get; set; }
  public List<Offer> List { get; set; }
  public string? Owner { get; set; }
  public List<string> ParcelIDs { get; set; }
  public string? ParcelTemplateID { get; set; }
  public decimal? PercentageCompleted { get; set; }
  public List<string> ProviderAccountIDs { get; set; }
  public List<string> ProviderCodes { get; set; }
  public List<string> ProviderServiceCodes { get; set; }
  public bool? Test { get; set; }
  public int? TotalOffersCompleted { get; set; }
  public int? TotalOffersRequested { get; set; }
  public string? UpdatedAt { get; set; }
  public string? Weight { get; set; }
  public string? Width { get; set; }
}

/// <summary>Order model</summary>
public class Order {
  public string? BuyerShipmentMethod { get; set; }
  public string? BuyerShippingCost { get; set; }
  public string? BuyerShippingCostCurrency { get; set; }
  public string? CreatedAt { get; set; }
  public string? Id { get; set; }
  public DbStringArray? ItemIDs { get; set; }
  public string? MerchantCode { get; set; }
  public string? Notes { get; set; }
  public string? OrderCode { get; set; }
  public string? OrderNumber { get; set; }
  public string? OrderStatus { get; set; }
  public string? OrganizationID { get; set; }
  public string? Owner { get; set; }
  public Shipment? Shipment { get; set; }
  public string? SourceCode { get; set; }
  public string? SourceIdentifier { get; set; }
  public bool? Test { get; set; }
  public string? TotalAmount { get; set; }
  public string? TotalAmountCurrency { get; set; }
  public string? TotalTax { get; set; }
  public string? UpdatedAt { get; set; }
}

/// <summary>Parcel model</summary>
public class Parcel {
  public string? Amount { get; set; }
  public string? AmountLocal { get; set; }
  public string? AmountLocalOld { get; set; }
  public string? AmountLocalTax { get; set; }
  public string? AmountLocalVat { get; set; }
  public string? AmountOld { get; set; }
  public string? AmountTax { get; set; }
  public string? AmountVat { get; set; }
  public string? Barcode { get; set; }
  public string? BonusBalance { get; set; }
  public string? CommercialInvoiceUrl { get; set; }
  public string? CreatedAt { get; set; }
  public string? Currency { get; set; }
  public string? CurrencyLocal { get; set; }
  public string? CustomsDeclaration { get; set; }
  /// <summary>Desi of parcel</summary>
  public string? Desi { get; set; }
  public string? DiscountRate { get; set; }
  /// <summary>Distance unit of parcel</summary>
  public string? DistanceUnit { get; set; }
  public string? Eta { get; set; }
  public JSONContent? Extra { get; set; }
  /// <summary>Height of parcel</summary>
  public string? Height { get; set; }
  public bool? HidePackageContentOnTag { get; set; }
  public string? Id { get; set; }
  public bool? InvoiceGenerated { get; set; }
  public string? InvoiceID { get; set; }
  public bool? IsMainParcel { get; set; }
  public List<string> ItemIDs { get; set; }
  public string? LabelFileType { get; set; }
  public string? LabelURL { get; set; }
  /// <summary>Length of parcel</summary>
  public string? Length { get; set; }
  /// <summary>Weight unit of parcel</summary>
  public string? MassUnit { get; set; }
  public JSONContent? Metadata { get; set; }
  /// <summary>Meta string to add additional info on your shipment/parcel</summary>
  public string? MetadataText { get; set; }
  public string? OldDesi { get; set; }
  public string? OldWeight { get; set; }
  public string? Owner { get; set; }
  public string? ParcelReferenceCode { get; set; }
  /// <summary>Instead of setting parcel size manually, you can set this to a predefined Parcel Template</summary>
  public string? ParcelTemplateID { get; set; }
  public bool? ProductPaymentOnDelivery { get; set; }
  public string? ProviderTotalAmount { get; set; }
  public string? QrCodeUrl { get; set; }
  public string? RefundInvoiceID { get; set; }
  public string? ResponsiveLabelURL { get; set; }
  public string? ShipmentDate { get; set; }
  public string? ShipmentID { get; set; }
  public string? StateCode { get; set; }
  public string? Template { get; set; }
  public bool? Test { get; set; }
  public string? TotalAmount { get; set; }
  public string? TotalAmountLocal { get; set; }
  /// <summary>Tracking number</summary>
  public string? TrackingNumber { get; set; }
  public Tracking? TrackingStatus { get; set; }
  public string? TrackingUrl { get; set; }
  public string? UpdatedAt { get; set; }
  /// <summary>If true, auto calculates total parcel size using the size of items</summary>
  public bool? UseDimensionsOfItems { get; set; }
  /// <summary>If true, auto calculates total parcel weight using the weight of items</summary>
  public bool? UseWeightOfItems { get; set; }
  /// <summary>Weight of parcel</summary>
  public string? Weight { get; set; }
  /// <summary>Width of parcel</summary>
  public string? Width { get; set; }
}

/// <summary>Shipment model</summary>
public class Shipment {
  public Offer? AcceptedOffer { get; set; }
  public string? AcceptedOfferID { get; set; }
  public string? Amount { get; set; }
  public string? AmountLocal { get; set; }
  public string? AmountLocalOld { get; set; }
  public string? AmountLocalTax { get; set; }
  public string? AmountLocalVat { get; set; }
  public string? AmountOld { get; set; }
  public string? AmountTax { get; set; }
  public string? AmountVat { get; set; }
  public string? Barcode { get; set; }
  public string? BonusBalance { get; set; }
  public string? BuyerNote { get; set; }
  public string? CancelDate { get; set; }
  public string? CategoryCode { get; set; }
  public string? CommercialInvoiceUrl { get; set; }
  public bool? CreateReturnLabel { get; set; }
  public string? CreatedAt { get; set; }
  public string? Currency { get; set; }
  public string? CurrencyLocal { get; set; }
  public string? CustomsDeclaration { get; set; }
  /// <summary>Desi of parcel</summary>
  public string? Desi { get; set; }
  public string? DiscountRate { get; set; }
  /// <summary>Distance unit of parcel</summary>
  public string? DistanceUnit { get; set; }
  public bool? EnableAutomation { get; set; }
  public string? Eta { get; set; }
  public List<Parcel> ExtraParcels { get; set; }
  public bool? HasError { get; set; }
  /// <summary>Height of parcel</summary>
  public string? Height { get; set; }
  public bool? HidePackageContentOnTag { get; set; }
  public string? Id { get; set; }
  public bool? InvoiceGenerated { get; set; }
  public string? InvoiceID { get; set; }
  public bool? IsRecipientSmsActivated { get; set; }
  public bool? IsReturn { get; set; }
  public bool? IsReturned { get; set; }
  public bool? IsTrackingOnly { get; set; }
  public List<Item> Items { get; set; }
  public string? LabelFileType { get; set; }
  public string? LabelURL { get; set; }
  public string? LastErrorCode { get; set; }
  public string? LastErrorMessage { get; set; }
  /// <summary>Length of parcel</summary>
  public string? Length { get; set; }
  /// <summary>Weight unit of parcel</summary>
  public string? MassUnit { get; set; }
  public JSONContent? Metadata { get; set; }
  /// <summary>Meta string to add additional info on your shipment/parcel</summary>
  public string? MetadataText { get; set; }
  public OfferList? Offers { get; set; }
  public string? OldDesi { get; set; }
  public string? OldWeight { get; set; }
  public Order? Order { get; set; }
  public string? OrderID { get; set; }
  public int? OrganizationShipmentID { get; set; }
  public string? Owner { get; set; }
  public string? PackageAcceptedAt { get; set; }
  /// <summary>Instead of setting parcel size manually, you can set this to a predefined Parcel Template</summary>
  public string? ParcelTemplateID { get; set; }
  public bool? ProductPaymentOnDelivery { get; set; }
  public string? ProviderAccountID { get; set; }
  public List<string> ProviderAccountIDs { get; set; }
  public string? ProviderBranchName { get; set; }
  public string? ProviderCode { get; set; }
  public List<string> ProviderCodes { get; set; }
  public string? ProviderInvoiceNo { get; set; }
  public string? ProviderReceiptNo { get; set; }
  public string? ProviderSerialNo { get; set; }
  public string? ProviderServiceCode { get; set; }
  public List<string> ProviderServiceCodes { get; set; }
  public string? ProviderTotalAmount { get; set; }
  public string? QrCodeUrl { get; set; }
  public Address? RecipientAddress { get; set; }
  public string? RecipientAddressID { get; set; }
  public string? RefundInvoiceID { get; set; }
  public string? ResponsiveLabelURL { get; set; }
  public string? ReturnAddressID { get; set; }
  public string? SellerNote { get; set; }
  public Address? SenderAddress { get; set; }
  public string? SenderAddressID { get; set; }
  public string? ShipmentDate { get; set; }
  public string? StatusCode { get; set; }
  public List<string> Tags { get; set; }
  public string? TenantId { get; set; }
  public bool? Test { get; set; }
  public string? TotalAmount { get; set; }
  public string? TotalAmountLocal { get; set; }
  /// <summary>Tracking number</summary>
  public string? TrackingNumber { get; set; }
  public Tracking? TrackingStatus { get; set; }
  public string? TrackingUrl { get; set; }
  public string? UpdatedAt { get; set; }
  /// <summary>If true, auto calculates total parcel size using the size of items</summary>
  public bool? UseDimensionsOfItems { get; set; }
  /// <summary>If true, auto calculates total parcel weight using the weight of items</summary>
  public bool? UseWeightOfItems { get; set; }
  /// <summary>Weight of parcel</summary>
  public string? Weight { get; set; }
  /// <summary>Width of parcel</summary>
  public string? Width { get; set; }
}

/// <summary>ShipmentResponse model</summary>
public class ShipmentResponse {
  public string? AdditionalMessage { get; set; }
  public string? Code { get; set; }
  public Shipment? Data { get; set; }
  public string? Message { get; set; }
  public bool? Result { get; set; }
}

/// <summary>Tracking model</summary>
public class Tracking {
  public string? CreatedAt { get; set; }
  public string? Hash { get; set; }
  public string? Id { get; set; }
  public decimal? LocationLat { get; set; }
  public decimal? LocationLng { get; set; }
  public string? LocationName { get; set; }
  public string? Owner { get; set; }
  public string? StatusDate { get; set; }
  public string? StatusDetails { get; set; }
  public bool? Test { get; set; }
  public string? TrackingStatusCode { get; set; }
  public string? TrackingSubStatusCode { get; set; }
  public string? UpdatedAt { get; set; }
}
