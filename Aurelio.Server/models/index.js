const sequelize = require('../config/database');
const VerificationCode = require('./verificationCode');
const Account = require('./account');

// 同步所有模型到数据库
async function syncModels() {
    try {
        // 使用 alter: true 选项来更新表结构而不是删除重建
        await sequelize.sync({ alter: true });
        console.log('数据库模型同步成功');
    } catch (error) {
        console.error('数据库模型同步失败:', error);
    }
}

module.exports = {
    sequelize,
    VerificationCode,
    Account,
    syncModels
}; 