

import 'dart:convert';

List<IncomeChartResponse> incomeChartResponseFromJson(String str) => List<IncomeChartResponse>.from(json.decode(str).map((x) => IncomeChartResponse.fromJson(x)));

String incomeChartResponseToJson(List<IncomeChartResponse> data) => json.encode(List<dynamic>.from(data.map((x) => x.toJson())));

class IncomeChartResponse {
  DateTime? time;
  num? totalPrice;

  IncomeChartResponse({
    this.time,
    this.totalPrice,
  });

  IncomeChartResponse copyWith({
    DateTime? time,
    num? totalPrice,
  }) =>
      IncomeChartResponse(
        time: time ?? this.time,
        totalPrice: totalPrice ?? this.totalPrice,
      );

  factory IncomeChartResponse.fromJson(Map<String, dynamic> json) => IncomeChartResponse(
    time: json["time"] == null ? null : DateTime.parse(json["time"].toString()),
    totalPrice: json["total_price"]?.toDouble(),
  );

  Map<String, dynamic> toJson() => {
    "time": "${time!.year.toString().padLeft(4, '0')}-${time!.month.toString().padLeft(2, '0')}-${time!.day.toString().padLeft(2, '0')}",
    "total_price": totalPrice,
  };
}
