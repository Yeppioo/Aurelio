const { VerificationCode } = require('../models');
const { Op } = require('sequelize');

// 清理过期的验证码
async function cleanupExpiredCodes() {
    try {
        const result = await VerificationCode.destroy({
            where: {
                expiresAt: {
                    [Op.lt]: new Date() // 删除所有过期时间小于当前时间的记录
                }
            }
        });
        console.log(`已清理 ${result} 条过期的验证码`);
    } catch (error) {
        console.error('清理过期验证码时发生错误:', error);
    }
}

// 启动定时清理任务
function startCleanupTask() {
    // 立即执行一次清理
    cleanupExpiredCodes();

    // 每30分钟执行一次验证码清理
    setInterval(cleanupExpiredCodes, 30 * 60 * 1000);

    console.log('验证码清理任务已启动，每30分钟执行一次');
}

module.exports = {
    startCleanupTask
}; 