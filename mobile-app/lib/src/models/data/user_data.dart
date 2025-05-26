import 'address_data.dart';
import 'shop_data.dart';
import 'currency_data.dart';

class UserData {
  UserData({
    int? id,
    int? ordersCount,
    num? ordersSumPrice,
    String? uuid,
    String? firstname,
    String? lastname,
    String? email,
    String? phone,
    String? birthday,
    String? gender,
    String? emailVerifiedAt,
    String? registeredAt,
    bool? active,
    String? img,
    String? role,
    List<AddressData>? addresses,
    ShopData? shop,
    Wallet? wallet,
    String? password,
    String? confirmPassword,
  }) {
    _id = id;
    _ordersCount = ordersCount;
    _ordersSumPrice = ordersSumPrice;
    _uuid = uuid;
    _firstname = firstname;
    _lastname = lastname;
    _email = email;
    _phone = phone;
    _birthday = birthday;
    _gender = gender;
    _emailVerifiedAt = emailVerifiedAt;
    _registeredAt = registeredAt;
    _active = active;
    _img = img;
    _role = role;
    _addresses = addresses;
    _shop = shop;
    _wallet = wallet;
  }

  UserData.fromJson(dynamic json) {
    _id = json['id'];
    _ordersCount = json["orders_count"];
    _ordersSumPrice = json["orders_sum_price"];
    _uuid = json['uuid'];
    _firstname = json['firstname'];
    _lastname = json['lastname'];
    _email = json['email'];
    _phone = json['phone'];
    _birthday = json['birthday'];
    _gender = json['gender'];
    _emailVerifiedAt = json['email_verified_at'];
    _registeredAt = json['registered_at'];
    _active = json['active'].runtimeType == int
        ? (json['active'] == 1)
        : json['active'];
    _role = json['role'];
    _img = json['img'];
    if (json['addresses'] != null) {
      _addresses = [];
      json['addresses'].forEach((v) {
        _addresses?.add(AddressData.fromJson(v));
      });
    }
    _shop = json['shop'] != null ? ShopData.fromJson(json['shop']) : null;
    _wallet = json['wallet'] != null ? Wallet.fromJson(json['wallet']) : null;
  }

  int? _id;
  int? _ordersCount;
  num? _ordersSumPrice;
  String? _uuid;
  String? _firstname;
  String? _lastname;
  String? _email;
  String? _phone;
  String? _birthday;
  String? _gender;
  String? _emailVerifiedAt;
  String? _registeredAt;
  bool? _active;
  String? _img;
  String? _role;
  List<AddressData>? _addresses;
  ShopData? _shop;
  Wallet? _wallet;
  String? _password;
  String? _confirmPassword;

  UserData copyWith({
    int? id,
    int? ordersCount,
    num? ordersSumPrice,
    String? uuid,
    String? firstname,
    String? lastname,
    String? email,
    String? phone,
    String? birthday,
    String? gender,
    String? emailVerifiedAt,
    String? registeredAt,
    bool? active,
    String? img,
    String? role,
    List<AddressData>? addresses,
    ShopData? shop,
    Wallet? wallet,
    String? password,
    String? conPassword,
  }) =>
      UserData(
        id: id ?? _id,
        ordersCount: ordersCount ?? _ordersCount,
        ordersSumPrice: ordersSumPrice ?? _ordersSumPrice,
        uuid: uuid ?? _uuid,
        firstname: firstname ?? _firstname,
        lastname: lastname ?? _lastname,
        email: email ?? _email,
        phone: phone ?? _phone,
        birthday: birthday ?? _birthday,
        gender: gender ?? _gender,
        emailVerifiedAt: emailVerifiedAt ?? _emailVerifiedAt,
        registeredAt: registeredAt ?? _registeredAt,
        active: active ?? _active,
        img: img ?? _img,
        role: role ?? _role,
        addresses: addresses ?? _addresses,
        shop: shop ?? _shop,
        wallet: wallet ?? _wallet,
        confirmPassword:  conPassword ?? _confirmPassword,
        password:  password ?? _password
      );

  int? get id => _id;

  String? get uuid => _uuid;

  String? get firstname => _firstname;

  String? get lastname => _lastname;

  String? get email => _email;

  String? get phone => _phone;

  String? get birthday => _birthday;

  String? get gender => _gender;

  String? get emailVerifiedAt => _emailVerifiedAt;

  String? get registeredAt => _registeredAt;

  bool? get active => _active;

  num? get ordersSumPrice => _ordersSumPrice;

  int? get ordersCount => _ordersCount;

  String? get img => _img;

  String? get role => _role;

  List<AddressData>? get addresses => _addresses;

  ShopData? get shop => _shop;

  Wallet? get wallet => _wallet;

  String? get password => _password;

  String? get conPassword => _confirmPassword;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['id'] = _id;
    map['uuid'] = _uuid;
    map['firstname'] = _firstname;
    map['lastname'] = _lastname;
    map['email'] = _email;
    map['phone'] = _phone;
    map['birthday'] = _birthday;
    map['gender'] = _gender;
    map['email_verified_at'] = _emailVerifiedAt;
    map['registered_at'] = _registeredAt;
    map['active'] = _active;
    map['img'] = _img;
    map['role'] = _role;
    if (_addresses != null) {
      map['addresses'] = _addresses?.map((v) => v.toJson()).toList();
    }
    if (_shop != null) {
      map['shop'] = _shop?.toJson();
    }
    if (_wallet != null) {
      map['wallet'] = _wallet?.toJson();
    }
    return map;
  }
}

class Wallet {
  Wallet({
    String? uuid,
    int? userId,
    int? currencyId,
    num? price,
    String? createdAt,
    String? updatedAt,
    CurrencyData? currency,
  }) {
    _uuid = uuid;
    _userId = userId;
    _currencyId = currencyId;
    _price = price;
    _createdAt = createdAt;
    _updatedAt = updatedAt;
    _currency = currency;
  }

  Wallet.fromJson(dynamic json) {
    _uuid = json['uuid'];
    _userId = json['user_id'];
    _currencyId = json['currency_id'];
    _price = json['price'];
    _createdAt = json['created_at'];
    _updatedAt = json['updated_at'];
    _currency = json['currency'] != null
        ? CurrencyData.fromJson(json['currency'])
        : null;
  }

  String? _uuid;
  int? _userId;
  int? _currencyId;
  num? _price;
  String? _createdAt;
  String? _updatedAt;
  CurrencyData? _currency;

  Wallet copyWith({
    String? uuid,
    int? userId,
    int? currencyId,
    num? price,
    String? createdAt,
    String? updatedAt,
    CurrencyData? currency,
  }) =>
      Wallet(
        uuid: uuid ?? _uuid,
        userId: userId ?? _userId,
        currencyId: currencyId ?? _currencyId,
        price: price ?? _price,
        createdAt: createdAt ?? _createdAt,
        updatedAt: updatedAt ?? _updatedAt,
        currency: currency ?? _currency,
      );

  String? get uuid => _uuid;

  int? get userId => _userId;

  int? get currencyId => _currencyId;

  num? get price => _price;

  String? get createdAt => _createdAt;

  String? get updatedAt => _updatedAt;

  CurrencyData? get currency => _currency;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['uuid'] = _uuid;
    map['user_id'] = _userId;
    map['currency_id'] = _currencyId;
    map['price'] = _price;
    map['created_at'] = _createdAt;
    map['updated_at'] = _updatedAt;
    if (_currency != null) {
      map['currency'] = _currency?.toJson();
    }
    return map;
  }
}
