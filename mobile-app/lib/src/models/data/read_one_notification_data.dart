
class ReadOneNotificationResponse {
    ReadOneNotificationModel? data;

    ReadOneNotificationResponse({
        this.data,
    });

    ReadOneNotificationResponse copyWith({
        ReadOneNotificationModel? data,
    }) => 
        ReadOneNotificationResponse(
            data: data ?? this.data,
        );

    factory ReadOneNotificationResponse.fromJson(Map<String, dynamic> json) => ReadOneNotificationResponse(
        data: json["data"] == null ? null : ReadOneNotificationModel.fromJson(json["data"]),
    );

    Map<String, dynamic> toJson() => {
        "data": data?.toJson(),
    };
}

class ReadOneNotificationModel {
    int? id;
    String? type;
    String? title;
    String? body;
    DataData? data;
    int? userId;
    DateTime? createdAt;
    DateTime? updatedAt;
    DateTime? readAt;
    User? user;
    Client? client;

    ReadOneNotificationModel({
        this.id,
        this.type,
        this.title,
        this.body,
        this.data,
        this.userId,
        this.createdAt,
        this.updatedAt,
        this.readAt,
        this.user,
        this.client,
    });

    ReadOneNotificationModel copyWith({
        int? id,
        String? type,
        String? title,
        String? body,
        DataData? data,
        int? userId,
        DateTime? createdAt,
        DateTime? updatedAt,
        DateTime? readAt,
        User? user,
        Client? client,
    }) => 
        ReadOneNotificationModel(
            id: id ?? this.id,
            type: type ?? this.type,
            title: title ?? this.title,
            body: body ?? this.body,
            data: data ?? this.data,
            userId: userId ?? this.userId,
            createdAt: createdAt ?? this.createdAt,
            updatedAt: updatedAt ?? this.updatedAt,
            readAt: readAt ?? this.readAt,
            user: user ?? this.user,
            client: client ?? this.client,
        );

    factory ReadOneNotificationModel.fromJson(Map<String, dynamic> json) => ReadOneNotificationModel(
        id: json["id"],
        type: json["type"],
        title: json["title"],
        body: json["body"],
        data: json["data"] == null ? null : DataData.fromJson(json["data"]),
        userId: json["user_id"],
        createdAt: json["created_at"] == null ? null : DateTime.parse(json["created_at"]),
        updatedAt: json["updated_at"] == null ? null : DateTime.parse(json["updated_at"]),
        readAt: json["read_at"] == null ? null : DateTime.parse(json["read_at"]),
        user: json["user"] == null ? null : User.fromJson(json["user"]),
        client: json["client"] == null ? null : Client.fromJson(json["client"]),
    );

    Map<String, dynamic> toJson() => {
        "id": id,
        "type": type,
        "title": title,
        "body": body,
        "data": data?.toJson(),
        "user_id": userId,
        "created_at": createdAt?.toIso8601String(),
        "updated_at": updatedAt?.toIso8601String(),
        "read_at": readAt?.toIso8601String(),
        "user": user?.toJson(),
        "client": client?.toJson(),
    };
}

class Client {
    int? id;
    String? firstname;
    String? lastname;
    bool? emptyP;
    int? active;
    String? role;

    Client({
        this.id,
        this.firstname,
        this.lastname,
        this.emptyP,
        this.active,
        this.role,
    });

    Client copyWith({
        int? id,
        String? firstname,
        String? lastname,
        bool? emptyP,
        int? active,
        String? role,
    }) => 
        Client(
            id: id ?? this.id,
            firstname: firstname ?? this.firstname,
            lastname: lastname ?? this.lastname,
            emptyP: emptyP ?? this.emptyP,
            active: active ?? this.active,
            role: role ?? this.role,
        );

    factory Client.fromJson(Map<String, dynamic> json) => Client(
        id: json["id"],
        firstname: json["firstname"],
        lastname: json["lastname"],
        emptyP: json["empty_p"],
        active: json["active"],
        role: json["role"],
    );

    Map<String, dynamic> toJson() => {
        "id": id,
        "firstname": firstname,
        "lastname": lastname,
        "empty_p": emptyP,
        "active": active,
        "role": role,
    };
}

class DataData {
    int? id;
    String? type;
    String? status;

    DataData({
        this.id,
        this.type,
        this.status,
    });

    DataData copyWith({
        int? id,
        String? type,
        String? status,
    }) => 
        DataData(
            id: id ?? this.id,
            type: type ?? this.type,
            status: status ?? this.status,
        );

    factory DataData.fromJson(Map<String, dynamic> json) => DataData(
        id: json["id"],
        type: json["type"],
        status: json["status"],
    );

    Map<String, dynamic> toJson() => {
        "id": id,
        "type": type,
        "status": status,
    };
}

class User {
    int? id;
    String? uuid;
    String? firstname;
    String? lastname;
    bool? emptyP;
    String? email;
    String? phone;
    DateTime? birthday;
    String? gender;
    int? active;
    String? img;
    String? myReferral;
    String? role;
    DateTime? emailVerifiedAt;
    DateTime? registeredAt;
    DateTime? createdAt;
    DateTime? updatedAt;

    User({
        this.id,
        this.uuid,
        this.firstname,
        this.lastname,
        this.emptyP,
        this.email,
        this.phone,
        this.birthday,
        this.gender,
        this.active,
        this.img,
        this.myReferral,
        this.role,
        this.emailVerifiedAt,
        this.registeredAt,
        this.createdAt,
        this.updatedAt,
    });

    User copyWith({
        int? id,
        String? uuid,
        String? firstname,
        String? lastname,
        bool? emptyP,
        String? email,
        String? phone,
        DateTime? birthday,
        String? gender,
        int? active,
        String? img,
        String? myReferral,
        String? role,
        DateTime? emailVerifiedAt,
        DateTime? registeredAt,
        DateTime? createdAt,
        DateTime? updatedAt,
    }) => 
        User(
            id: id ?? this.id,
            uuid: uuid ?? this.uuid,
            firstname: firstname ?? this.firstname,
            lastname: lastname ?? this.lastname,
            emptyP: emptyP ?? this.emptyP,
            email: email ?? this.email,
            phone: phone ?? this.phone,
            birthday: birthday ?? this.birthday,
            gender: gender ?? this.gender,
            active: active ?? this.active,
            img: img ?? this.img,
            myReferral: myReferral ?? this.myReferral,
            role: role ?? this.role,
            emailVerifiedAt: emailVerifiedAt ?? this.emailVerifiedAt,
            registeredAt: registeredAt ?? this.registeredAt,
            createdAt: createdAt ?? this.createdAt,
            updatedAt: updatedAt ?? this.updatedAt,
        );

    factory User.fromJson(Map<String, dynamic> json) => User(
        id: json["id"],
        uuid: json["uuid"],
        firstname: json["firstname"],
        lastname: json["lastname"],
        emptyP: json["empty_p"],
        email: json["email"],
        phone: json["phone"],
        birthday: json["birthday"] == null ? null : DateTime.parse(json["birthday"]),
        gender: json["gender"],
        active: json["active"],
        img: json["img"],
        myReferral: json["my_referral"],
        role: json["role"],
        emailVerifiedAt: json["email_verified_at"] == null ? null : DateTime.parse(json["email_verified_at"]),
        registeredAt: json["registered_at"] == null ? null : DateTime.parse(json["registered_at"]),
        createdAt: json["created_at"] == null ? null : DateTime.parse(json["created_at"]),
        updatedAt: json["updated_at"] == null ? null : DateTime.parse(json["updated_at"]),
    );

    Map<String, dynamic> toJson() => {
        "id": id,
        "uuid": uuid,
        "firstname": firstname,
        "lastname": lastname,
        "empty_p": emptyP,
        "email": email,
        "phone": phone,
        "birthday": birthday?.toIso8601String(),
        "gender": gender,
        "active": active,
        "img": img,
        "my_referral": myReferral,
        "role": role,
        "email_verified_at": emailVerifiedAt?.toIso8601String(),
        "registered_at": registeredAt?.toIso8601String(),
        "created_at": createdAt?.toIso8601String(),
        "updated_at": updatedAt?.toIso8601String(),
    };
}
