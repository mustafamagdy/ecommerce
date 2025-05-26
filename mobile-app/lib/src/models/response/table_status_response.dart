import 'package:admin_desktop/src/models/response/table_statistics.dart';

import '../data/table_data.dart';

class TableResponse {
  String? timestamp;
  bool? status;
  String? message;
  Data? data;

  TableResponse({this.timestamp, this.status, this.message, this.data});

  TableResponse.fromJson(Map<String, dynamic> json) {
    timestamp = json['timestamp'];
    status = json['status'];
    message = json['message'];
    data = json['data'] != null ? Data.fromJson(json['data']) : null;
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['timestamp'] = timestamp;
    data['status'] = status;
    data['message'] = message;
    if (this.data != null) {
      data['data'] = this.data!.toJson();
    }
    return data;
  }
}

class Data {
  TableStatistic? statistic;
  List<TableData>? tables;
  Meta? meta;

  Data({this.statistic, this.tables, this.meta});

  Data.fromJson(Map<String, dynamic> json) {
    statistic = json['statistic'] != null ? TableStatistic.fromJson(json['statistic']) : null;
    if (json['tables'] != null) {
      tables = <TableData>[];
      json['tables'].forEach((v) { tables!.add(TableData.fromJson(v)); });
    }
    meta = json['meta'] != null ? Meta.fromJson(json['meta']) : null;
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    if (statistic != null) {
      data['statistic'] = statistic!.toJson();
    }
    if (tables != null) {
      data['tables'] = tables!.map((v) => v.toJson()).toList();
    }
    if (meta != null) {
      data['meta'] = meta!.toJson();
    }

    return data;
  }
}


class Meta {
  int? currentPage;
  String? perPage;
  int? lastPage;
  int? total;
  int? from;
  int? to;

  Meta({this.currentPage, this.perPage, this.lastPage, this.total, this.from, this.to});

  Meta.fromJson(Map<String, dynamic> json) {
    currentPage = json['current_page'];
    perPage = json['per_page'];
    lastPage = json['last_page'];
    total = json['total'];
    from = json['from'];
    to = json['to'];
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data =  <String, dynamic>{};
    data['current_page'] = currentPage;
    data['per_page'] = perPage;
    data['last_page'] = lastPage;
    data['total'] = total;
    data['from'] = from;
    data['to'] = to;
    return data;
  }
}


