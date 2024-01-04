import 'translation.dart';
import 'location.dart';
import 'shop_delivery.dart';

class ShopData {
  ShopData({
    int? id,
    String? uuid,
    int? userId,
    num? tax,
    num? deliveryRange,
    num? percentage,
    Location? location,
    String? phone,
    String? openTime,
    String? closeTime,
    String? backgroundImg,
    String? logoImg,
    num? minAmount,
    String? status,
    String? statusNote,
    String? ratingAvg,
    String? createdAt,
    String? updatedAt,
    dynamic deletedAt,
    Translation? translation,
    Seller? seller,
    List<ShopDelivery>? deliveries,
  }) {
    _id = id;
    _uuid = uuid;
    _userId = userId;
    _tax = tax;
    _deliveryRange = deliveryRange;
    _percentage = percentage;
    _location = location;
    _phone = phone;
    _showType = showType;
    _open = open;
    _visibility = visibility;
    _openTime = openTime;
    _closeTime = closeTime;
    _backgroundImg = backgroundImg;
    _logoImg = logoImg;
    _minAmount = minAmount;
    _status = status;
    _statusNote = statusNote;
    _ratingAvg = ratingAvg;
    _createdAt = createdAt;
    _updatedAt = updatedAt;
    _deletedAt = deletedAt;
    _translation = translation;
    _seller = seller;
    _deliveries = deliveries;
  }

  ShopData.fromJson(dynamic json) {
    _id = json['id'];
    _uuid = json['uuid'];
    _userId = json['user_id'];
    _tax = json['tax'];
    _deliveryRange = json['delivery_range'];
    _percentage = json['percentage'];
    _location =
    json['location'] != null ? Location.fromJson(json['location']) : null;
    _phone = json['phone'];
    _openTime = json['open_time'];
    _closeTime = json['close_time'];
    _backgroundImg = json['background_img'];
    _logoImg = json['logo_img'];
    _minAmount = json['min_amount'];
    _status = json['status'];
    _statusNote = json['status_note'];
    _ratingAvg = json['rating_avg'];
    _createdAt = json['created_at'];
    _updatedAt = json['updated_at'];
    _deletedAt = json['deleted_at'];
    _translation = json['translation'] != null
        ? Translation.fromJson(json['translation'])
        : null;
    _seller = json['seller'] != null ? Seller.fromJson(json['seller']) : null;
    if (json['deliveries'] != null) {
      _deliveries = [];
      json['deliveries'].forEach((v) {
        _deliveries?.add(ShopDelivery.fromJson(v));
      });
    }
  }

  int? _id;
  String? _uuid;
  int? _userId;
  num? _tax;
  num? _deliveryRange;
  num? _percentage;
  Location? _location;
  String? _phone;
  bool? _showType;
  bool? _open;
  bool? _visibility;
  String? _openTime;
  String? _closeTime;
  String? _backgroundImg;
  String? _logoImg;
  num? _minAmount;
  String? _status;
  String? _statusNote;
  String? _ratingAvg;
  String? _createdAt;
  String? _updatedAt;
  dynamic _deletedAt;
  Translation? _translation;
  Seller? _seller;
  List<ShopDelivery>? _deliveries;

  ShopData copyWith({
    int? id,
    String? uuid,
    int? userId,
    num? tax,
    num? deliveryRange,
    num? percentage,
    Location? location,
    String? phone,
    bool? showType,
    bool? open,
    bool? visibility,
    String? openTime,
    String? closeTime,
    String? backgroundImg,
    String? logoImg,
    num? minAmount,
    String? status,
    String? statusNote,
    String? ratingAvg,
    String? createdAt,
    String? updatedAt,
    dynamic deletedAt,
    Translation? translation,
    Seller? seller,
    List<ShopDelivery>? deliveries,
  }) =>
      ShopData(
        id: id ?? _id,
        uuid: uuid ?? _uuid,
        userId: userId ?? _userId,
        tax: tax ?? _tax,
        deliveryRange: deliveryRange ?? _deliveryRange,
        percentage: percentage ?? _percentage,
        location: location ?? _location,
        phone: phone ?? _phone,
        openTime: openTime ?? _openTime,
        closeTime: closeTime ?? _closeTime,
        backgroundImg: backgroundImg ?? _backgroundImg,
        logoImg: logoImg ?? _logoImg,
        minAmount: minAmount ?? _minAmount,
        status: status ?? _status,
        statusNote: statusNote ?? _statusNote,
        ratingAvg: ratingAvg ?? _ratingAvg,
        createdAt: createdAt ?? _createdAt,
        updatedAt: updatedAt ?? _updatedAt,
        deletedAt: deletedAt ?? _deletedAt,
        translation: translation ?? _translation,
        seller: seller ?? _seller,
        deliveries: deliveries ?? _deliveries,
      );

  int? get id => _id;

  String? get uuid => _uuid;

  int? get userId => _userId;

  num? get tax => _tax;

  num? get deliveryRange => _deliveryRange;

  num? get percentage => _percentage;

  Location? get location => _location;

  String? get phone => _phone;

  bool? get showType => _showType;

  bool? get open => _open;

  bool? get visibility => _visibility;

  String? get openTime => _openTime;

  String? get closeTime => _closeTime;

  String? get backgroundImg => _backgroundImg;

  String? get logoImg => _logoImg;

  num? get minAmount => _minAmount;

  String? get status => _status;

  String? get statusNote => _statusNote;

  String? get ratingAvg => _ratingAvg;

  String? get createdAt => _createdAt;

  String? get updatedAt => _updatedAt;

  dynamic get deletedAt => _deletedAt;

  Translation? get translation => _translation;

  Seller? get seller => _seller;

  List<ShopDelivery>? get deliveries => _deliveries;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['id'] = _id;
    map['uuid'] = _uuid;
    map['user_id'] = _userId;
    map['tax'] = _tax;
    map['delivery_range'] = _deliveryRange;
    map['percentage'] = _percentage;
    if (_location != null) {
      map['location'] = _location?.toJson();
    }
    map['phone'] = _phone;
    map['show_type'] = _showType;
    map['open'] = _open;
    map['visibility'] = _visibility;
    map['open_time'] = _openTime;
    map['close_time'] = _closeTime;
    map['background_img'] = _backgroundImg;
    map['logo_img'] = _logoImg;
    map['min_amount'] = _minAmount;
    map['status'] = _status;
    map['status_note'] = _statusNote;
    map['rating_avg'] = _ratingAvg;
    map['created_at'] = _createdAt;
    map['updated_at'] = _updatedAt;
    map['deleted_at'] = _deletedAt;
    if (_translation != null) {
      map['translation'] = _translation?.toJson();
    }
    if (_seller != null) {
      map['seller'] = _seller?.toJson();
    }
    if (_deliveries != null) {
      map['deliveries'] = _deliveries?.map((v) => v.toJson()).toList();
    }
    return map;
  }
}

class Seller {
  Seller({
    int? id,
    String? firstname,
    String? lastname,
    String? role,
  }) {
    _id = id;
    _firstname = firstname;
    _lastname = lastname;
    _role = role;
  }

  Seller.fromJson(dynamic json) {
    _id = json['id'];
    _firstname = json['firstname'];
    _lastname = json['lastname'];
    _role = json['role'];
  }

  int? _id;
  String? _firstname;
  String? _lastname;
  String? _role;

  Seller copyWith({
    int? id,
    String? firstname,
    String? lastname,
    String? role,
  }) =>
      Seller(
        id: id ?? _id,
        firstname: firstname ?? _firstname,
        lastname: lastname ?? _lastname,
        role: role ?? _role,
      );

  int? get id => _id;

  String? get firstname => _firstname;

  String? get lastname => _lastname;

  String? get role => _role;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['id'] = _id;
    map['firstname'] = _firstname;
    map['lastname'] = _lastname;
    map['role'] = _role;
    return map;
  }
}
