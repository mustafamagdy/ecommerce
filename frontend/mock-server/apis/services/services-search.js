module.exports = function(req, res) {
    res.send({
        "data": [
            {
                "id": "5feb8468-c647-478f-905e-42f537d7bd08",
                "name": "غسيل",
                "description": " ,وصف الخدمة",
                "imageUrl": "https://fakeimg.pl/300/"
            },
            {
                "id": "5feb8468-c647-478f-905e-42f537d7bd09",
                "name": "كي",
                "description": "وصف الخدمة",
                "imageUrl": "https://fakeimg.pl/300/"
            },
            {
                "id": "5feb8468-c647-478f-905e-42f537d7bd18",
                "name": "غسيل وكي",
                "description": "وصف الخدمة",
                "imageUrl": "https://fakeimg.pl/300/"
            },
            {
                "id": "5feb8468-c647-478f-905e-42f537d7bd28",
                "name": "غسيل مستعجل",
                "description": "وصف الخدمة",
                "imageUrl": "https://fakeimg.pl/300/"
            },
            {
                "id": "5feb8468-c647-478f-905e-42f537d7bd38",
                "name": "غسيل فائق",
                "description": "وصف الخدمة",
                "imageUrl": "https://fakeimg.pl/300/"
            }
        ],
        "currentPage": 1,
        "totalPages": 1,
        "totalCount": 4,
        "pageSize": 10,
        "hasPreviousPage": false,
        "hasNextPage": false
    });
};
