# Changelog

Bu dosya SDK'daki önemli değişiklikleri listeler.

This file documents notable changes in the SDK.

## Sürüm / Version

- Türkçe: Bu değişiklikler `1.2.0` sürümünde yer alır.
- English: These changes are included in version `1.2.0`.

## Türkçe

### 1.2.0

- Tüm yeni gönderilerde alıcı adresi zorunludur; gönderici adresi verilmezse kurumun varsayılan gönderici adresi kullanılır.
- `Shipments.CreateAsync(...)` ile `IsReturn` ve gönderici/alıcı için bağımsız ID veya satır içi adres seçimi kullanılarak iade gönderisi oluşturulabilir.
- `examples/IncomingShipment` eklendi.

### 1.1.3

#### Eklendi

- `Transactions.CreateReturnAsync(...)` ile ayrık bir iade + etiketi hemen satın alma akışı eklendi.
- İki yeni iade örneği eklendi:
  - iadeyi oluşturup etiketi sonra satın alma
  - iadeyi oluşturup etiketi hemen satın alma

#### Değişti

- `Shipments.CreateReturnAsync(...)` artık sadece shipment döndüren iade oluşturma akışıdır; etiketi satın almaz.
- İade dokümanı iki akışı ayrı anlatır:
  - iadeyi oluşturup etiketi satın almama
  - iadeyi oluşturup etiketi hemen satın alma
- README ve örnekler, etiketin daha sonra `Transactions.AcceptOfferAsync(...)` ile satın alınabileceğini açıklar.

## English

### 1.2.0

- All new shipments require a recipient address; the organization default sender address is used when no sender is provided.
- `Shipments.CreateAsync(...)` supports independent returns with `IsReturn`; the backend reverses the delivery direction for carriers.
- Added `examples/IncomingShipment`.

### 1.1.3

#### Added

- Added a dedicated return-transaction flow via `Transactions.CreateReturnAsync(...)`.
- Added return examples for:
  - creating a return and purchasing the label later
  - creating a return and purchasing the label immediately

#### Changed

- `Shipments.CreateReturnAsync(...)` is now shipment-only. It creates the return shipment without purchasing the label.
- Return documentation now separates:
  - return creation without label purchase
  - return creation with immediate label purchase
- Return examples and README guidance now document that label purchase can be done later with `Transactions.AcceptOfferAsync(...)`.
