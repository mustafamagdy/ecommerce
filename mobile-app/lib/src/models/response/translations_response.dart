class TranslationsResponse {
  TranslationsResponse({
    String? timestamp,
    bool? status,
    String? message,
    Map<String, dynamic>? data,
  }) {
    _timestamp = timestamp;
    _status = status;
    _message = message;
    _data = data;
  }

  TranslationsResponse.fromJson(dynamic json) {
    _timestamp = json['timestamp'];
    _status = json['status'];
    _message = json['message'];
    _data = json['data'];
  }

  String? _timestamp;
  bool? _status;
  String? _message;
  Map<String, dynamic>? _data;

  TranslationsResponse copyWith({
    String? timestamp,
    bool? status,
    String? message,
    Map<String, dynamic>? data,
  }) =>
      TranslationsResponse(
        timestamp: timestamp ?? _timestamp,
        status: status ?? _status,
        message: message ?? _message,
        data: data ?? _data,
      );

  String? get timestamp => _timestamp;

  bool? get status => _status;

  String? get message => _message;

  Map<String, dynamic>? get data => _data;
}
