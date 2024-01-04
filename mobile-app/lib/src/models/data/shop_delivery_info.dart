import 'shop_data.dart';
import 'shop_delivery.dart';

class ShopDeliveryInfo {
  final ShopData? shop;
  final String? deliveryDate;
  final String? deliveryTime;
  final List<ShopDelivery>? shopDeliveries;
  final ShopDelivery? selectedShopDelivery;

  ShopDeliveryInfo({
    this.shop,
    this.deliveryDate,
    this.deliveryTime,
    this.shopDeliveries,
    this.selectedShopDelivery,
  });

  ShopDeliveryInfo copyWith({
    ShopData? shop,
    String? deliveryDate,
    String? deliveryTime,
    List<ShopDelivery>? shopDeliveries,
    ShopDelivery? selectedShopDelivery,
  }) =>
      ShopDeliveryInfo(
        shop: shop ?? this.shop,
        deliveryDate: deliveryDate ?? this.deliveryDate,
        deliveryTime: deliveryTime ?? this.deliveryTime,
        shopDeliveries: shopDeliveries ?? this.shopDeliveries,
        selectedShopDelivery: selectedShopDelivery ?? this.selectedShopDelivery,
      );
}
