import '../data/user_data.dart';

class UsersPaginateResponse {
  UsersPaginateResponse({List<UserData>? users}) {
    _users = users;
  }

  UsersPaginateResponse.fromJson(dynamic json) {
    if (json['data'] != null) {
      _users = [];
      json['data'].forEach((v) {
        _users?.add(UserData.fromJson(v));
      });
    }
  }

  List<UserData>? _users;

  UsersPaginateResponse copyWith({List<UserData>? users}) =>
      UsersPaginateResponse(users: users ?? _users);

  List<UserData>? get users => _users;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    if (_users != null) {
      map['data'] = _users?.map((v) => v.toJson()).toList();
    }
    return map;
  }
}
