const { Sequelize } = require('sequelize');

const sequelize = new Sequelize('Aurelio', 'Yep', 'Yep210190', {
    host: '120.25.233.220',
    port: 3306,
    dialect: 'mysql',
    logging: false,
});

module.exports = sequelize; 