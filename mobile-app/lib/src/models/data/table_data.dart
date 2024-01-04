
class TableData {
  int? id;
  String? name;
  int? shopSectionId;
  int? tax;
  int? chairCount;
  bool? active;
  String? createdAt;
  String? updatedAt;
  ShopSection? shopSection;

  TableData({this.id, this.name, this.shopSectionId, this.tax, this.chairCount, this.active, this.createdAt, this.updatedAt, this.shopSection});

  TableData.fromJson(Map<String, dynamic> json) {
    id = json['id'];
    name = json['name'];
    shopSectionId = json['shop_section_id'];
    tax = json['tax'];
    chairCount = json['chair_count'];
    active = json['active'];
    createdAt = json['created_at'];
    updatedAt = json['updated_at'];
    shopSection = json['shop_section'] != null ? ShopSection.fromJson(json['shop_section']) : null;
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['id'] = id;
    data['name'] = name;
    data['shop_section_id'] = shopSectionId;
    data['tax'] = tax;
    data['chair_count'] = chairCount;
    data['active'] = active;
    data['created_at'] = createdAt;
    data['updated_at'] = updatedAt;
    if (shopSection != null) {
      data['shop_section'] = shopSection!.toJson();
    }
    return data;
  }
}

class ShopSection {
  int? id;
  int? shopId;
  String? area;
  String? img;
  String? createdAt;
  String? updatedAt;
  Translation? translation;

  ShopSection({this.id, this.shopId, this.area, this.img, this.createdAt, this.updatedAt, this.translation});

  ShopSection.fromJson(Map<String, dynamic> json) {
    id = json['id'];
    shopId = json['shop_id'];
    area = json['area'];
    img = json['img'];
    createdAt = json['created_at'];
    updatedAt = json['updated_at'];
    translation = json['translation'] != null ? Translation.fromJson(json['translation']) : null;
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['id'] = id;
    data['shop_id'] = shopId;
    data['area'] = area;
    data['img'] = img;
    data['created_at'] = createdAt;
    data['updated_at'] = updatedAt;
    if (translation != null) {
      data['translation'] = translation!.toJson();
    }
    return data;
  }
}

class Translation {
  int? id;
  String? locale;
  String? title;

  Translation({this.id, this.locale, this.title});

  Translation.fromJson(Map<String, dynamic> json) {
    id = json['id'];
    locale = json['locale'];
    title = json['title'];
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['id'] = id;
    data['locale'] = locale;
    data['title'] = title;
    return data;
  }
}




