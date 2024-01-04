import 'package:admin_desktop/src/core/utils/local_storage.dart';
import 'package:admin_desktop/src/models/data/bag_data.dart';
import 'package:admin_desktop/src/models/data/location_data.dart';

class OrderBodyData {
  final String? note;
  final int userId;
  final num deliveryFee;
  final int currencyId;
  final num rate;
  final String deliveryType;
  final String? coupon;
  final LocationData location;
  final AddressModel address;
  final String deliveryDate;
  final String deliveryTime;
  final BagData bagData;

  OrderBodyData({
    required this.currencyId,
    required this.rate,
    required this.userId,
    required this.deliveryFee,
    required this.deliveryType,
    this.coupon,
    required this.location,
    required this.address,
    required this.deliveryDate,
    required this.deliveryTime,
    this.note,
    required this.bagData,
  });

  Map toJson() {
    Map newMap = {};
    List<BagProductData> listOfProduct = [];
    for (int i = 0; i < (bagData.bagProducts?.length ?? 0); i++) {
      listOfProduct.add(BagProductData(
          stockId: bagData.bagProducts?[i].stockId,
          quantity: bagData.bagProducts?[i].quantity));
      for (int j = 0; j < (bagData.bagProducts?[i].carts?.length ?? 0); j++) {
        listOfProduct.add(BagProductData(
            stockId: bagData.bagProducts?[i].carts?[j].stockId,
            quantity: bagData.bagProducts?[i].carts?[j].quantity,
            parentId: bagData.bagProducts?[i].carts?[j].parentId));
      }
    }
    newMap["currency_id"] = currencyId;
    newMap["rate"] = rate;
    newMap["shop_id"] = LocalStorage.instance.getUser()?.shop?.id ?? 0;
    if (userId != 0) newMap["user_id"] = userId;
    if (deliveryFee != 0) newMap["delivery_fee"] = deliveryFee;
    newMap["delivery_type"] = deliveryType.toLowerCase();
    if (coupon != null && (coupon?.isNotEmpty ?? false)) {
      newMap["coupon"] = coupon;
    }
    if (note != null && (note?.isNotEmpty ?? false)) newMap["comment"] = note;
    newMap["location"] = location.toJson();
    newMap["address"] = address.toJson();
    newMap["delivery_date"] = deliveryDate;
    newMap["delivery_time"] = deliveryTime;
    newMap['products'] = listOfProduct;
    return newMap;
  }
}

class AddressModel {
  final String? address;
  final String? office;
  final String? house;
  final String? floor;

  AddressModel({
    this.address,
    this.office,
    this.house,
    this.floor,
  });

  Map toJson() {
    return {
      "address": address,
      "office": office,
      "house": house,
      "floor": floor
    };
  }

  factory AddressModel.fromJson(Map? data) {
    return AddressModel(
      address: data?["address"],
      office: data?["office"],
      house: data?["house"],
      floor: data?["floor"],
    );
  }
}

class ProductOrder {
  final int stockId;
  final num price;
  final int quantity;
  final num tax;
  final num discount;
  final num totalPrice;

  ProductOrder({
    required this.stockId,
    required this.price,
    required this.quantity,
    required this.tax,
    required this.discount,
    required this.totalPrice,
  });

  @override
  String toString() {
    return "{\"stock_id\":$stockId, \"price\":$price, \"qty\":$quantity, \"tax\":$tax, \"discount\":$discount, \"total_price\":$totalPrice}";
  }
}
