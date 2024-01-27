import 'package:admin_desktop/src/core/constants/app_constants.dart';

class LocationData {
  LocationData({double? latitude, double? longitude}) {
    _latitude = latitude;
    _longitude = longitude;
  }

  LocationData.fromJson(dynamic json) {
    final lat = json['latitude'];
    final lon = json['longitude'];
    if (lat != null) {
      _latitude = double.tryParse(lat.toString());
    }
    if (lon != null) {
      _longitude = double.tryParse(lon.toString());
    }


}

double? _latitude;
double? _longitude;

LocationData copyWith({double? latitude, double? longitude}) =>
    LocationData(
      latitude: latitude ?? _latitude,
      longitude: longitude ?? _longitude,
    );

double? get latitude => _latitude;

double? get longitude => _longitude;

Map<String, dynamic> toJson() {
  final map = <String, dynamic>{};
  map['latitude'] = _latitude ?? AppConstants.demoLatitude;
  map['longitude'] = _longitude ?? AppConstants.demoLongitude;
  return map;
}}
