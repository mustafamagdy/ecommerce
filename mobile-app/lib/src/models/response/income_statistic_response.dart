

class IncomeStatisticResponse {
  StatisticModel? incomeStatisticResponseNew;
  StatisticModel? accepted;
  StatisticModel? ready;
  StatisticModel? onAWay;
  StatisticModel? delivered;
  StatisticModel? canceled;
  Group? group;

  IncomeStatisticResponse({
    this.incomeStatisticResponseNew,
    this.accepted,
    this.ready,
    this.onAWay,
    this.delivered,
    this.canceled,
    this.group,
  });

  IncomeStatisticResponse copyWith({
    StatisticModel? incomeStatisticResponseNew,
    StatisticModel? accepted,
    StatisticModel? ready,
    StatisticModel? onAWay,
    StatisticModel? delivered,
    StatisticModel? canceled,
    Group? group,
  }) =>
      IncomeStatisticResponse(
        incomeStatisticResponseNew: incomeStatisticResponseNew ?? this.incomeStatisticResponseNew,
        accepted: accepted ?? this.accepted,
        ready: ready ?? this.ready,
        onAWay: onAWay ?? this.onAWay,
        delivered: delivered ?? this.delivered,
        canceled: canceled ?? this.canceled,
        group: group ?? this.group,
      );

  factory IncomeStatisticResponse.fromJson(Map<String, dynamic> json) => IncomeStatisticResponse(
    incomeStatisticResponseNew: json["new"] == null ? null : StatisticModel.fromJson(json["new"]),
    accepted: json["accepted"] == null ? null : StatisticModel.fromJson(json["accepted"]),
    ready: json["ready"] == null ? null : StatisticModel.fromJson(json["ready"]),
    onAWay: json["on_a_way"] == null ? null : StatisticModel.fromJson(json["on_a_way"]),
    delivered: json["delivered"] == null ? null : StatisticModel.fromJson(json["delivered"]),
    canceled: json["canceled"] == null ? null : StatisticModel.fromJson(json["canceled"]),
    group: json["group"] == null ? null : Group.fromJson(json["group"]),
  );

  Map<String, dynamic> toJson() => {
    "new": incomeStatisticResponseNew?.toJson(),
    "accepted": accepted?.toJson(),
    "ready": ready?.toJson(),
    "on_a_way": onAWay?.toJson(),
    "delivered": delivered?.toJson(),
    "canceled": canceled?.toJson(),
    "group": group?.toJson(),
  };
}

class StatisticModel {
  num? sum;
  num? percent;

  StatisticModel({
    this.sum,
    this.percent,
  });

  StatisticModel copyWith({
    num? sum,
    num? percent,
  }) =>
      StatisticModel(
        sum: sum ?? this.sum,
        percent: percent ?? this.percent,
      );

  factory StatisticModel.fromJson(Map<String, dynamic> json) => StatisticModel(
    sum: json["sum"],
    percent: json["percent"]?.toDouble(),
  );

  Map<String, dynamic> toJson() => {
    "sum": sum,
    "percent": percent,
  };
}

class Group {
  StatisticModel? active;
  StatisticModel? completed;
  StatisticModel? ended;

  Group({
    this.active,
    this.completed,
    this.ended,
  });

  Group copyWith({
    StatisticModel? active,
    StatisticModel? completed,
    StatisticModel? ended,
  }) =>
      Group(
        active: active ?? this.active,
        completed: completed ?? this.completed,
        ended: ended ?? this.ended,
      );

  factory Group.fromJson(Map<String, dynamic> json) => Group(
    active: json["active"] == null ? null : StatisticModel.fromJson(json["active"]),
    completed: json["completed"] == null ? null : StatisticModel.fromJson(json["completed"]),
    ended: json["ended"] == null ? null : StatisticModel.fromJson(json["ended"]),
  );

  Map<String, dynamic> toJson() => {
    "active": active?.toJson(),
    "completed": completed?.toJson(),
    "ended": ended?.toJson(),
  };
}
