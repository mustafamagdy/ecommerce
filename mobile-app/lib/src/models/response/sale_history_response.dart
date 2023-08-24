

import 'package:admin_desktop/src/models/data/user_data.dart';

class SaleHistoryResponse {
  List<SaleHistoryModel>? list;

  SaleHistoryResponse({
    this.list,
  });

  SaleHistoryResponse copyWith({
    List<SaleHistoryModel>? data,
  }) =>
      SaleHistoryResponse(
        list: data ?? list,
      );

  factory SaleHistoryResponse.fromJson(Map<String, dynamic> json) => SaleHistoryResponse(
    list: json["data"] == null ? [] : List<SaleHistoryModel>.from(json["data"]!.map((x) => SaleHistoryModel.fromJson(x))),
  );

  Map<String, dynamic> toJson() => {
    "data": list == null ? [] : List<dynamic>.from(list!.map((x) => x.toJson())),
  };
}

class SaleHistoryModel {
  int? id;
  int? userId;
  num? totalPrice;
  DateTime? createdAt;
  String? note;
  UserData? user;
  List<Transaction>? transactions;

  SaleHistoryModel({
    this.id,
    this.userId,
    this.totalPrice,
    this.createdAt,
    this.note,
    this.user,
    this.transactions,
  });

  SaleHistoryModel copyWith({
    int? id,
    int? userId,
    double? totalPrice,
    DateTime? createdAt,
    String? note,
    UserData? user,
    List<Transaction>? transactions,
  }) =>
      SaleHistoryModel(
        id: id ?? this.id,
        userId: userId ?? this.userId,
        totalPrice: totalPrice ?? this.totalPrice,
        createdAt: createdAt ?? this.createdAt,
        note: note ?? this.note,
        user: user ?? this.user,
        transactions: transactions ?? this.transactions,
      );

  factory SaleHistoryModel.fromJson(Map<String, dynamic> json) => SaleHistoryModel(
    id: json["id"],
    userId: json["user_id"],
    totalPrice: json["total_price"]?.toDouble(),
    createdAt: json["created_at"] == null ? null : DateTime.parse(json["created_at"]),
    note: json["note"],
    user: json["user"] == null ? null : UserData.fromJson(json["user"]),
    transactions: json["transactions"] == null ? [] : List<Transaction>.from(json["transactions"]!.map((x) => Transaction.fromJson(x))),
  );

  Map<String, dynamic> toJson() => {
    "id": id,
    "user_id": userId,
    "total_price": totalPrice,
    "created_at": createdAt?.toIso8601String(),
    "note": note,
    "user": user?.toJson(),
    "transactions": transactions == null ? [] : List<dynamic>.from(transactions!.map((x) => x.toJson())),
  };
}

class Transaction {
  int? id;
  int? payableId;
  String? payableType;
  String? status;
  int? paymentSysId;
  PaymentSystem? paymentSystem;

  Transaction({
    this.id,
    this.payableId,
    this.payableType,
    this.status,
    this.paymentSysId,
    this.paymentSystem,
  });

  Transaction copyWith({
    int? id,
    int? payableId,
    String? payableType,
    String? status,
    int? paymentSysId,
    PaymentSystem? paymentSystem,
  }) =>
      Transaction(
        id: id ?? this.id,
        payableId: payableId ?? this.payableId,
        payableType: payableType ?? this.payableType,
        status: status ?? this.status,
        paymentSysId: paymentSysId ?? this.paymentSysId,
        paymentSystem: paymentSystem ?? this.paymentSystem,
      );

  factory Transaction.fromJson(Map<String, dynamic> json) => Transaction(
    id: json["id"],
    payableId: json["payable_id"],
    payableType: json["payable_type"],
    status: json["status"],
    paymentSysId: json["payment_sys_id"],
    paymentSystem: json["payment_system"] == null ? null : PaymentSystem.fromJson(json["payment_system"]),
  );

  Map<String, dynamic> toJson() => {
    "id": id,
    "payable_id": payableId,
    "payable_type": payableType,
    "status": status,
    "payment_sys_id": paymentSysId,
    "payment_system": paymentSystem?.toJson(),
  };
}

class PaymentSystem {
  int? id;
  String? tag;

  PaymentSystem({
    this.id,
    this.tag,
  });

  PaymentSystem copyWith({
    int? id,
    String? tag,
  }) =>
      PaymentSystem(
        id: id ?? this.id,
        tag: tag ?? this.tag,
      );

  factory PaymentSystem.fromJson(Map<String, dynamic> json) => PaymentSystem(
    id: json["id"],
    tag: json["tag"],
  );

  Map<String, dynamic> toJson() => {
    "id": id,
    "tag": tag,
  };
}

