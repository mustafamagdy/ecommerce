class TableStatistic {
  int? available;
  int? booked;
  int? occupied;

  TableStatistic({this.available, this.booked, this.occupied});

  TableStatistic.fromJson(Map<String, dynamic> json) {
    available = json['available'];
    booked = json['booked'];
    occupied = json['occupied'];
  }

  Map<String, dynamic> toJson() {
    final Map<String, dynamic> data = <String, dynamic>{};
    data['available'] = available;
    data['booked'] = booked;
    data['occupied'] = occupied;
    return data;
  }
}