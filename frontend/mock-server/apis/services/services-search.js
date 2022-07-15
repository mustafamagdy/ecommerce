const faker = require("@faker-js/faker").faker;
module.exports = function (req, res) {
    let arr = [];
    for (let i = 0; i < 10; i++) {
        arr.push({
            "id": faker.datatype.uuid(),
            "name": faker.fake(`اسم الخدمة {{random.numeric(3)}}`),
            "description": faker.lorem.words(20),
            "imageUrl": faker.image.abstract(300, 300)
        });
    }
    const totalCount = 174;
    const totalPages = totalCount > req.body.pageSize ? Math.ceil(totalCount / req.body.pageSize) : 1;
    res.send({
        "data": arr,
        "currentPage": req.body.pageNumber,
        "totalPages": totalPages,
        "totalCount": totalCount,
        "pageSize": req.pageSize,
        "hasPreviousPage": req.body.pageNumber !== 1,
        "hasNextPage": req.body.pageNumber !== totalPages
    });
};
