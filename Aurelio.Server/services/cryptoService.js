const crypto = require('crypto');

// 生成随机盐值
function generateSalt(length = 16) {
    return crypto.randomBytes(length).toString('hex');
}

// 使用SHA256进行密码哈希
function hashPassword(password, salt) {
    const hash = crypto.createHash('sha256');
    hash.update(password + salt);
    return hash.digest('hex');
}

// 验证密码
function verifyPassword(password, salt, hashedPassword) {
    const computedHash = hashPassword(password, salt);
    return computedHash === hashedPassword;
}

module.exports = {
    generateSalt,
    hashPassword,
    verifyPassword
}; 