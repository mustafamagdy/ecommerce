

class IncomeCartResponse {
  num? revenue;
  num? revenuePercent;
  num? orders;
  num? ordersPercent;
  num? average;
  num? averagePercent;
  String? revenueType;
  String? ordersType;
  String? averageType;

  IncomeCartResponse({
    this.revenue,
    this.revenuePercent,
    this.orders,
    this.ordersPercent,
    this.average,
    this.averagePercent,
    this.revenueType,
    this.ordersType,
    this.averageType,
  });

  IncomeCartResponse copyWith({
    num? revenue,
    num? revenuePercent,
    num? orders,
    num? ordersPercent,
    num? average,
    num? averagePercent,
    String? revenueType,
    String? ordersType,
    String? averageType,
  }) =>
      IncomeCartResponse(
        revenue: revenue ?? this.revenue,
        revenuePercent: revenuePercent ?? this.revenuePercent,
        orders: orders ?? this.orders,
        ordersPercent: ordersPercent ?? this.ordersPercent,
        average: average ?? this.average,
        averagePercent: averagePercent ?? this.averagePercent,
        revenueType: revenueType ?? this.revenueType,
        ordersType: ordersType ?? this.ordersType,
        averageType: averageType ?? this.averageType,
      );

  factory IncomeCartResponse.fromJson(Map<String, dynamic> json) => IncomeCartResponse(
    revenue: json["revenue"],
    revenuePercent: json["revenue_percent"],
    orders: json["orders"],
    ordersPercent: json["orders_percent"],
    average: json["average"],
    averagePercent: json["average_percent"],
    revenueType: json["revenue_percent_type"],
    ordersType: json["orders_percent_type"],
    averageType: json["average_percent_type"],
  );

  Map<String, dynamic> toJson() => {
    "revenue": revenue,
    "revenue_percent": revenuePercent,
    "orders": orders,
    "orders_percent": ordersPercent,
    "average": average,
    "average_percent": averagePercent,
  };
}
