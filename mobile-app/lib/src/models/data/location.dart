class Location {
  Location({String? latitude, String? longitude}) {
    _latitude = latitude;
    _longitude = longitude;
  }

  Location.fromJson(dynamic json) {
    _latitude = json['latitude'];
    _longitude = json['longitude'];
  }

  String? _latitude;
  String? _longitude;

  Location copyWith({String? latitude, String? longitude}) => Location(
    latitude: latitude ?? _latitude,
    longitude: longitude ?? _longitude,
  );

  String? get latitude => _latitude;

  String? get longitude => _longitude;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['latitude'] = _latitude;
    map['longitude'] = _longitude;
    return map;
  }
}
