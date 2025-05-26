import '../data/user_data.dart';

class LoginResponse {
  LoginResponse({
    String? timestamp,
    bool? status,
    String? message,
    User? data,
  }) {
    _timestamp = timestamp;
    _status = status;
    _message = message;
    _data = data;
  }

  LoginResponse.fromJson(dynamic json) {
    _timestamp = json['timestamp'];
    _status = json['status'].runtimeType == int
        ? (json['status'] == 1)
        : json['status'];
    _message = json['message'];
    _data = json['data'] != null ? User.fromJson(json['data']) : null;
  }

  String? _timestamp;
  bool? _status;
  String? _message;
  User? _data;

  LoginResponse copyWith({
    String? timestamp,
    bool? status,
    String? message,
    User? data,
  }) =>
      LoginResponse(
        timestamp: timestamp ?? _timestamp,
        status: status ?? _status,
        message: message ?? _message,
        data: data ?? _data,
      );

  String? get timestamp => _timestamp;

  bool? get status => _status;

  String? get message => _message;

  User? get data => _data;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['timestamp'] = _timestamp;
    map['status'] = _status;
    map['message'] = _message;
    if (_data != null) {
      map['data'] = _data?.toJson();
    }
    return map;
  }
}

class User {
  User({
    String? accessToken,
    String? tokenType,
    UserData? user,
  }) {
    _accessToken = accessToken;
    _tokenType = tokenType;
    _user = user;
  }

  User.fromJson(dynamic json) {
    _accessToken = json['access_token'];
    _tokenType = json['token_type'];
    _user = json['user'] != null ? UserData.fromJson(json['user']) : null;
  }

  String? _accessToken;
  String? _tokenType;
  UserData? _user;

  User copyWith({
    String? accessToken,
    String? tokenType,
    UserData? user,
  }) =>
      User(
        accessToken: accessToken ?? _accessToken,
        tokenType: tokenType ?? _tokenType,
        user: user ?? _user,
      );

  String? get accessToken => _accessToken;

  String? get tokenType => _tokenType;

  UserData? get user => _user;

  Map<String, dynamic> toJson() {
    final map = <String, dynamic>{};
    map['access_token'] = _accessToken;
    map['token_type'] = _tokenType;
    if (_user != null) {
      map['user'] = _user?.toJson();
    }
    return map;
  }
}
