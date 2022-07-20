const faker = require("@faker-js/faker").faker;
module.exports = function (req, res) {
    let arr = [];
    const totalCount = 174;
    const pageSize = req.body.pageSize > 0 ? req.body.pageSize : totalCount;
    const totalPages = totalCount > pageSize ? Math.ceil(totalCount / pageSize) : 1;
    for (let i = 0; i < pageSize; i++) {
        arr.push({
            "id": faker.datatype.uuid(),
            "name": faker.fake(`اسم الخدمة {{random.numeric(3)}}`),
            "description": faker.lorem.words(20),
            "imageUrl": faker.image.abstract(300, 300)
        });
    }
    res.send({
        "data": arr,
        "currentPage": req.body.pageNumber,
        "totalPages": totalPages,
        "totalCount": totalCount,
        "pageSize": pageSize,
        "hasPreviousPage": req.body.pageNumber !== 1,
        "hasNextPage": req.body.pageNumber !== totalPages
    });
};
