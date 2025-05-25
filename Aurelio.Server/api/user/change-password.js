const express = require('express');
const router = express.Router();
const { Account } = require('../../models');
const { verifyPassword, generateSalt, hashPassword } = require('../../services/cryptoService');
const ErrorCode = require('../../config/errorCode');

// 密码格式验证函数
function validatePassword(password) {
    // 检查空格和不可见字符
    if (/\s/.test(password)) {
        return {
            isValid: false,
            ...ErrorCode.PASSWORD_FORMAT_INVALID
        };
    }

    // 检查密码长度
    if (password.length < 6 || password.length > 20) {
        return {
            isValid: false,
            ...ErrorCode.PASSWORD_LENGTH_INVALID
        };
    }

    // 检查是否只包含合法字符（字母、数字和常用特殊字符）
    if (!/^[A-Za-z0-9!@#$%^&*()_+\-=[\]{};':",./<>?]+$/.test(password)) {
        return {
            isValid: false,
            ...ErrorCode.PASSWORD_CHARS_INVALID
        };
    }

    return {
        isValid: true
    };
}

router.post('/', async (req, res) => {
    try {
        const { email, oldPassword, newPassword } = req.body;

        if (!email || !oldPassword || !newPassword) {
            return res.status(400).json({
                ...ErrorCode.PARAMS_EMPTY,
                error: '邮箱、旧密码和新密码不能为空'
            });
        }

        // 密码格式验证
        const passwordValidation = validatePassword(newPassword);
        if (!passwordValidation.isValid) {
            return res.status(400).json({
                code: passwordValidation.code,
                message: passwordValidation.message
            });
        }

        // 查找用户账户
        const userAccount = await Account.findOne({
            where: { email }
        });

        if (!userAccount) {
            return res.status(400).json(ErrorCode.USER_NOT_EXIST);
        }

        // 验证旧密码
        const isOldPasswordValid = verifyPassword(oldPassword, userAccount.salt, userAccount.password);

        if (!isOldPasswordValid) {
            return res.status(400).json(ErrorCode.PASSWORD_INVALID);
        }

        // 生成新的盐值和密码哈希
        const newSalt = generateSalt();
        const newHashedPassword = hashPassword(newPassword, newSalt);

        // 更新密码
        await userAccount.update({
            password: newHashedPassword,
            salt: newSalt
        });

        res.json({
            code: 200,
            message: '密码修改成功'
        });

    } catch (error) {
        console.error('修改密码过程发生错误:', error);
        res.status(500).json(ErrorCode.SERVER_ERROR);
    }
});

module.exports = router; 