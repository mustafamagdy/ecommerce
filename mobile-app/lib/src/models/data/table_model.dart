class TableModel {
  final String title;
  final int count;
  final int tax;
  final String zona;
  final bool isActive;

  TableModel({
    required this.title,
    required this.count,
    required this.tax,
    required this.zona,
     this.isActive = false,
  });
}
