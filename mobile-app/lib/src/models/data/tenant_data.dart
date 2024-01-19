// import 'address_data.dart';
// import 'shop_data.dart';
// import 'currency_data.dart';

class TenantData {
  TenantData({
    String? id,
    String? name,
  }) {
    _id = id;
    _name = name;
  }

  TenantData.fromJson(dynamic json) {
    _id = json['id'];
    _name = json["name"];
  }

  String? _id;
  String? _name;

  TenantData copyWith({
    String? id,
    String? name,
  }) =>
      TenantData(
        id: id ?? _id,
        name: name ?? _name,
      );

  String? get id => _id;

  String? get name => _name;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['id'] = _id;
    map['name'] = _name;
    return map;
  }
}
