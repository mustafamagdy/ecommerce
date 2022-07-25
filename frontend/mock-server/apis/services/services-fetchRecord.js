const faker = require("@faker-js/faker").faker;
module.exports = function (req, res) {
    const id = req.params.id;
    if (id && id !== "") {
        res.send({
            "id": id,
            "name": faker.fake(`اسم الخدمة {{random.numeric(3)}}`),
            "description": faker.lorem.words(20),
            "imageUrl": faker.image.abstract(300, 300)
        });
    }
};
