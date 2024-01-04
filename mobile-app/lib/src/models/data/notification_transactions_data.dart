// To parse this JSON data, do
//
//     final transactionModel = transactionModelFromJson(jsonString);

class TransactionListResponse {
  List<TransactionModel>? data;
  Links? links;
  Meta? meta;

  TransactionListResponse({
    this.data,
    this.links,
    this.meta,
  });

  TransactionListResponse copyWith({
    List<TransactionModel>? data,
    Links? links,
    Meta? meta,
  }) =>
      TransactionListResponse(
        data: data ?? this.data,
        links: links ?? this.links,
        meta: meta ?? this.meta,
      );

  factory TransactionListResponse.fromJson(Map<String, dynamic> json) =>
      TransactionListResponse(
        data: json["data"] == null
            ? []
            : List<TransactionModel>.from(
                json["data"]!.map((x) => TransactionModel.fromJson(x))),
        // links: json["links"] == null ? null : Links.fromJson(json["links"]),
        // meta: json["meta"] == null ? null : Meta.fromJson(json["meta"]),
      );

  Map<String, dynamic> toJson() => {
        "data": data == null
            ? []
            : List<dynamic>.from(data!.map((x) => x.toJson())),
        "links": links?.toJson(),
        "meta": meta?.toJson(),
      };
}

class TransactionModel {
  int? id;
  int? payableId;
  double? price;
  dynamic paymentTrxId;
  String? note;
  DateTime? performTime;
  String? status;
  String? statusDescription;
  DateTime? createdAt;
  DateTime? updatedAt;
  User? user;
  PaymentSystem? paymentSystem;
  Payable? payable;

  TransactionModel({
    this.id,
    this.payableId,
    this.price,
    this.paymentTrxId,
    this.note,
    this.performTime,
    this.status,
    this.statusDescription,
    this.createdAt,
    this.updatedAt,
    this.user,
    this.paymentSystem,
    this.payable,
  });

  TransactionModel copyWith({
    int? id,
    int? payableId,
    double? price,
    dynamic paymentTrxId,
    String? note,
    DateTime? performTime,
    String? status,
    String? statusDescription,
    DateTime? createdAt,
    DateTime? updatedAt,
    User? user,
    PaymentSystem? paymentSystem,
    Payable? payable,
  }) =>
      TransactionModel(
        id: id ?? this.id,
        payableId: payableId ?? this.payableId,
        price: price ?? this.price,
        paymentTrxId: paymentTrxId ?? this.paymentTrxId,
        note: note ?? this.note,
        performTime: performTime ?? this.performTime,
        status: status ?? this.status,
        statusDescription: statusDescription ?? this.statusDescription,
        createdAt: createdAt ?? this.createdAt,
        updatedAt: updatedAt ?? this.updatedAt,
        user: user ?? this.user,
        paymentSystem: paymentSystem ?? this.paymentSystem,
        payable: payable ?? this.payable,
      );

  factory TransactionModel.fromJson(Map<String, dynamic> json) =>
      TransactionModel(
        id: json["id"],
        payableId: json["payable_id"],
        price: json["price"]?.toDouble(),
        paymentTrxId: json["payment_trx_id"],
        note: json["note"],
        performTime: json["perform_time"] == null
            ? null
            : DateTime.parse(json["perform_time"]),
        status: json["status"],
        statusDescription: json["status_description"],
        createdAt: json["created_at"] == null
            ? null
            : DateTime.parse(json["created_at"]),
        updatedAt: json["updated_at"] == null
            ? null
            : DateTime.parse(json["updated_at"]),
        user: json["user"] == null ? null : User.fromJson(json["user"]),
        paymentSystem: json["payment_system"] == null
            ? null
            : PaymentSystem.fromJson(json["payment_system"]),
        payable:
            json["payable"] == null ? null : Payable.fromJson(json["payable"]),
      );

  Map<String, dynamic> toJson() => {
        "id": id,
        "payable_id": payableId,
        "price": price,
        "payment_trx_id": paymentTrxId,
        "note": note,
        "perform_time": performTime?.toIso8601String(),
        "status": status,
        "status_description": statusDescription,
        "created_at": createdAt?.toIso8601String(),
        "updated_at": updatedAt?.toIso8601String(),
        "user": user?.toJson(),
        "payment_system": paymentSystem?.toJson(),
        "payable": payable?.toJson(),
      };
}

class Payable {
  int? id;
  int? userId;
  double? totalPrice;
  double? originPrice;
  int? rate;
  double? tax;
  double? commissionFee;
  String? status;
  Location? location;
  Address? address;
  String? deliveryType;
  DateTime? deliveryDate;
  String? deliveryTime;
  bool? current;
  DateTime? createdAt;
  DateTime? updatedAt;
  double? deliveryFee;

  Payable({
    this.id,
    this.userId,
    this.totalPrice,
    this.originPrice,
    this.rate,
    this.tax,
    this.commissionFee,
    this.status,
    this.location,
    this.address,
    this.deliveryType,
    this.deliveryDate,
    this.deliveryTime,
    this.current,
    this.createdAt,
    this.updatedAt,
    this.deliveryFee,
  });

  Payable copyWith({
    int? id,
    int? userId,
    double? totalPrice,
    double? originPrice,
    int? rate,
    double? tax,
    double? commissionFee,
    String? status,
    Location? location,
    Address? address,
    String? deliveryType,
    DateTime? deliveryDate,
    String? deliveryTime,
    bool? current,
    DateTime? createdAt,
    DateTime? updatedAt,
    double? deliveryFee,
  }) =>
      Payable(
        id: id ?? this.id,
        userId: userId ?? this.userId,
        totalPrice: totalPrice ?? this.totalPrice,
        originPrice: originPrice ?? this.originPrice,
        rate: rate ?? this.rate,
        tax: tax ?? this.tax,
        commissionFee: commissionFee ?? this.commissionFee,
        status: status ?? this.status,
        location: location ?? this.location,
        address: address ?? this.address,
        deliveryType: deliveryType ?? this.deliveryType,
        deliveryDate: deliveryDate ?? this.deliveryDate,
        deliveryTime: deliveryTime ?? this.deliveryTime,
        current: current ?? this.current,
        createdAt: createdAt ?? this.createdAt,
        updatedAt: updatedAt ?? this.updatedAt,
        deliveryFee: deliveryFee ?? this.deliveryFee,
      );

  factory Payable.fromJson(Map<String, dynamic> json) => Payable(
        id: json["id"],
        userId: json["user_id"],
        totalPrice: json["total_price"]?.toDouble(),
        originPrice: json["origin_price"]?.toDouble(),
        rate: json["rate"],
        tax: json["tax"]?.toDouble(),
        commissionFee: json["commission_fee"]?.toDouble(),
        status: json["status"],
        location: json["location"] == null
            ? null
            : Location.fromJson(json["location"]),
        address:
            json["address"] == null ? null : Address.fromJson(json["address"]),
        deliveryType: json["delivery_type"],
        deliveryDate: json["delivery_date"] == null
            ? null
            : DateTime.parse(json["delivery_date"]),
        deliveryTime: json["delivery_time"],
        current: json["current"],
        createdAt: json["created_at"] == null
            ? null
            : DateTime.parse(json["created_at"]),
        updatedAt: json["updated_at"] == null
            ? null
            : DateTime.parse(json["updated_at"]),
        deliveryFee: json["delivery_fee"]?.toDouble(),
      );

  Map<String, dynamic> toJson() => {
        "id": id,
        "user_id": userId,
        "total_price": totalPrice,
        "origin_price": originPrice,
        "rate": rate,
        "tax": tax,
        "commission_fee": commissionFee,
        "status": status,
        "location": location?.toJson(),
        "address": address?.toJson(),
        "delivery_type": deliveryType,
        "delivery_date":
            "${deliveryDate!.year.toString().padLeft(4, '0')}-${deliveryDate!.month.toString().padLeft(2, '0')}-${deliveryDate!.day.toString().padLeft(2, '0')}",
        "delivery_time": deliveryTime,
        "current": current,
        "created_at": createdAt?.toIso8601String(),
        "updated_at": updatedAt?.toIso8601String(),
        "delivery_fee": deliveryFee,
      };
}

class Address {
  String? floor;
  String? house;
  String? office;
  String? address;

  Address({
    this.floor,
    this.house,
    this.office,
    this.address,
  });

  Address copyWith({
    String? floor,
    String? house,
    String? office,
    String? address,
  }) =>
      Address(
        floor: floor ?? this.floor,
        house: house ?? this.house,
        office: office ?? this.office,
        address: address ?? this.address,
      );

  factory Address.fromJson(Map<String, dynamic> json) => Address(
        floor: json["floor"],
        house: json["house"],
        office: json["office"],
        address: json["address"],
      );

  Map<String, dynamic> toJson() => {
        "floor": floor,
        "house": house,
        "office": office,
        "address": address,
      };
}

class Location {
  double? latitude;
  double? longitude;

  Location({
    this.latitude,
    this.longitude,
  });

  Location copyWith({
    double? latitude,
    double? longitude,
  }) =>
      Location(
        latitude: latitude ?? this.latitude,
        longitude: longitude ?? this.longitude,
      );

  factory Location.fromJson(Map<String, dynamic> json) => Location(
        latitude: json["latitude"]?.toDouble(),
        longitude: json["longitude"]?.toDouble(),
      );

  Map<String, dynamic> toJson() => {
        "latitude": latitude,
        "longitude": longitude,
      };
}

class PaymentSystem {
  int? id;
  String? tag;
  bool? active;
  DateTime? createdAt;
  DateTime? updatedAt;
  int? input;

  PaymentSystem({
    this.id,
    this.tag,
    this.active,
    this.createdAt,
    this.updatedAt,
    this.input,
  });

  PaymentSystem copyWith({
    int? id,
    String? tag,
    bool? active,
    DateTime? createdAt,
    DateTime? updatedAt,
    int? input,
  }) =>
      PaymentSystem(
        id: id ?? this.id,
        tag: tag ?? this.tag,
        active: active ?? this.active,
        createdAt: createdAt ?? this.createdAt,
        updatedAt: updatedAt ?? this.updatedAt,
        input: input ?? this.input,
      );

  factory PaymentSystem.fromJson(Map<String, dynamic> json) => PaymentSystem(
        id: json["id"],
        tag: json["tag"],
        active: json["active"],
        createdAt: json["created_at"] == null
            ? null
            : DateTime.parse(json["created_at"]),
        updatedAt: json["updated_at"] == null
            ? null
            : DateTime.parse(json["updated_at"]),
        input: json["input"],
      );

  Map<String, dynamic> toJson() => {
        "id": id,
        "tag": tag,
        "active": active,
        "created_at": createdAt?.toIso8601String(),
        "updated_at": updatedAt?.toIso8601String(),
        "input": input,
      };
}

class User {
  int? id;
  String? uuid;
  String? firstname;
  String? lastname;
  bool? emptyP;
  String? email;
  String? gender;
  int? active;
  String? myReferral;
  String? role;
  DateTime? emailVerifiedAt;
  DateTime? registeredAt;
  DateTime? createdAt;
  DateTime? updatedAt;
  String? phone;
  DateTime? phoneVerifiedAt;
  DateTime? birthday;
  String? img;

  User({
    this.id,
    this.uuid,
    this.firstname,
    this.lastname,
    this.emptyP,
    this.email,
    this.gender,
    this.active,
    this.myReferral,
    this.role,
    this.emailVerifiedAt,
    this.registeredAt,
    this.createdAt,
    this.updatedAt,
    this.phone,
    this.phoneVerifiedAt,
    this.birthday,
    this.img,
  });

  User copyWith({
    int? id,
    String? uuid,
    String? firstname,
    String? lastname,
    bool? emptyP,
    String? email,
    String? gender,
    int? active,
    String? myReferral,
    String? role,
    DateTime? emailVerifiedAt,
    DateTime? registeredAt,
    DateTime? createdAt,
    DateTime? updatedAt,
    String? phone,
    DateTime? phoneVerifiedAt,
    DateTime? birthday,
    String? img,
  }) =>
      User(
        id: id ?? this.id,
        uuid: uuid ?? this.uuid,
        firstname: firstname ?? this.firstname,
        lastname: lastname ?? this.lastname,
        emptyP: emptyP ?? this.emptyP,
        email: email ?? this.email,
        gender: gender ?? this.gender,
        active: active ?? this.active,
        myReferral: myReferral ?? this.myReferral,
        role: role ?? this.role,
        emailVerifiedAt: emailVerifiedAt ?? this.emailVerifiedAt,
        registeredAt: registeredAt ?? this.registeredAt,
        createdAt: createdAt ?? this.createdAt,
        updatedAt: updatedAt ?? this.updatedAt,
        phone: phone ?? this.phone,
        phoneVerifiedAt: phoneVerifiedAt ?? this.phoneVerifiedAt,
        birthday: birthday ?? this.birthday,
        img: img ?? this.img,
      );

  factory User.fromJson(Map<String, dynamic> json) => User(
        id: json["id"],
        uuid: json["uuid"],
        firstname: json["firstname"],
        lastname: json["lastname"],
        emptyP: json["empty_p"],
        email: json["email"],
        gender: json["gender"],
        active: json["active"],
        myReferral: json["my_referral"],
        role: json["role"],
        emailVerifiedAt: json["email_verified_at"] == null
            ? null
            : DateTime.parse(json["email_verified_at"]),
        registeredAt: json["registered_at"] == null
            ? null
            : DateTime.parse(json["registered_at"]),
        createdAt: json["created_at"] == null
            ? null
            : DateTime.parse(json["created_at"]),
        updatedAt: json["updated_at"] == null
            ? null
            : DateTime.parse(json["updated_at"]),
        phone: json["phone"],
        phoneVerifiedAt: json["phone_verified_at"] == null
            ? null
            : DateTime.parse(json["phone_verified_at"]),
        birthday:
            json["birthday"] == null ? null : DateTime.parse(json["birthday"]),
        img: json["img"],
      );

  Map<String, dynamic> toJson() => {
        "id": id,
        "uuid": uuid,
        "firstname": firstname,
        "lastname": lastname,
        "empty_p": emptyP,
        "email": email,
        "gender": gender,
        "active": active,
        "my_referral": myReferral,
        "role": role,
        "email_verified_at": emailVerifiedAt?.toIso8601String(),
        "registered_at": registeredAt?.toIso8601String(),
        "created_at": createdAt?.toIso8601String(),
        "updated_at": updatedAt?.toIso8601String(),
        "phone": phone,
        "phone_verified_at": phoneVerifiedAt?.toIso8601String(),
        "birthday": birthday?.toIso8601String(),
        "img": img,
      };
}

class Links {
  String? first;
  String? last;
  dynamic prev;
  String? next;

  Links({
    this.first,
    this.last,
    this.prev,
    this.next,
  });

  Links copyWith({
    String? first,
    String? last,
    dynamic prev,
    String? next,
  }) =>
      Links(
        first: first ?? this.first,
        last: last ?? this.last,
        prev: prev ?? this.prev,
        next: next ?? this.next,
      );

  factory Links.fromJson(Map<String, dynamic> json) => Links(
        first: json["first"],
        last: json["last"],
        prev: json["prev"],
        next: json["next"],
      );

  Map<String, dynamic> toJson() => {
        "first": first,
        "last": last,
        "prev": prev,
        "next": next,
      };
}

class Meta {
  int? currentPage;
  int? from;
  int? lastPage;
  List<Link>? links;
  String? path;
  int? perPage;
  int? to;
  int? total;

  Meta({
    this.currentPage,
    this.from,
    this.lastPage,
    this.links,
    this.path,
    this.perPage,
    this.to,
    this.total,
  });

  Meta copyWith({
    int? currentPage,
    int? from,
    int? lastPage,
    List<Link>? links,
    String? path,
    int? perPage,
    int? to,
    int? total,
  }) =>
      Meta(
        currentPage: currentPage ?? this.currentPage,
        from: from ?? this.from,
        lastPage: lastPage ?? this.lastPage,
        links: links ?? this.links,
        path: path ?? this.path,
        perPage: perPage ?? this.perPage,
        to: to ?? this.to,
        total: total ?? this.total,
      );

  factory Meta.fromJson(Map<String, dynamic> json) => Meta(
        currentPage: json["current_page"],
        from: json["from"],
        lastPage: json["last_page"],
        links: json["links"] == null
            ? []
            : List<Link>.from(json["links"]!.map((x) => Link.fromJson(x))),
        path: json["path"],
        perPage: json["per_page"],
        to: json["to"],
        total: json["total"],
      );

  Map<String, dynamic> toJson() => {
        "current_page": currentPage,
        "from": from,
        "last_page": lastPage,
        "links": links == null
            ? []
            : List<dynamic>.from(links!.map((x) => x.toJson())),
        "path": path,
        "per_page": perPage,
        "to": to,
        "total": total,
      };
}

class Link {
  String? url;
  String? label;
  bool? active;

  Link({
    this.url,
    this.label,
    this.active,
  });

  Link copyWith({
    String? url,
    String? label,
    bool? active,
  }) =>
      Link(
        url: url ?? this.url,
        label: label ?? this.label,
        active: active ?? this.active,
      );

  factory Link.fromJson(Map<String, dynamic> json) => Link(
        url: json["url"],
        label: json["label"],
        active: json["active"],
      );

  Map<String, dynamic> toJson() => {
        "url": url,
        "label": label,
        "active": active,
      };
}
