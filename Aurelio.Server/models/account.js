const { DataTypes } = require('sequelize');
const sequelize = require('../config/database');

// 生成随机字符串的函数
function generateRandomString(length) {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    let result = '';
    for (let i = 0; i < length; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

const Account = sequelize.define('Account', {
    id: {
        type: DataTypes.BIGINT,
        primaryKey: true,
        autoIncrement: true
    },
    email: {
        type: DataTypes.STRING,
        allowNull: false,
        unique: true,
        validate: {
            isEmail: true
        }
    },
    password: {
        type: DataTypes.STRING(64), // SHA256 produces a 64 character hex string
        allowNull: false,
    },
    salt: {
        type: DataTypes.STRING(32), // 16 bytes = 32 hex characters
        allowNull: false
    },
    username: {
        type: DataTypes.STRING,
        allowNull: false,
        defaultValue: function () {
            return '用户_' + generateRandomString(5);
        }
    },
    avatarUrl: {
        type: DataTypes.STRING,
        allowNull: false,
        defaultValue: 'https://tse2-mm.cn.bing.net/th/id/OIP-C.g5M-iZUiocFCi9YAzojtRAAAAA?cb=iwp2&rs=1&pid=ImgDetMain'
    },
    createdAt: {
        type: DataTypes.DATE,
        defaultValue: DataTypes.NOW
    },
    updatedAt: {
        type: DataTypes.DATE,
        defaultValue: DataTypes.NOW
    }
});

module.exports = Account; 