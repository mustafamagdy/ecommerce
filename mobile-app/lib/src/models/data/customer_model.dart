class CustomerModel {
  String? firstname;
  String? lastname;
  String? email;
  int? phone;

  String? role;

  CustomerModel(
      {this.firstname, this.lastname, this.email, this.phone, this.role});

  CustomerModel copyWith({
    String? firstname,
    String? lastname,
    String? email,
    int? phone,
    String? role,
  }) =>
      CustomerModel(
        firstname: firstname ?? this.firstname,
        lastname: lastname ?? this.lastname,
        email: email ?? this.email,
        phone: phone ?? this.phone,
        role: role ?? this.role,
      );

  factory CustomerModel.fromJson(Map<String, dynamic> json) => CustomerModel(
        firstname: json["firstname"],
        lastname: json["lastname"],
        email: json["email"],
        phone: json["phone"],
        role: json["role"],
      );

  Map<String, dynamic> toJson() => {
        "firstname": firstname,
        "lastname": lastname,
        "email": email,
        "phone": phone,
        "role": role,
      };
}
