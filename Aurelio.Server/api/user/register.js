const express = require('express');
const router = express.Router();
const { VerificationCode, Account } = require('../../models');
const { Op } = require('sequelize');
const { generateSalt, hashPassword } = require('../../services/cryptoService');
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
        const { email, code, password } = req.body;

        if (!email || !code || !password) {
            return res.status(400).json({
                ...ErrorCode.PARAMS_EMPTY,
                error: '邮箱、验证码和密码不能为空'
            });
        }

        // 密码格式验证
        const passwordValidation = validatePassword(password);
        if (!passwordValidation.isValid) {
            return res.status(400).json({
                code: passwordValidation.code,
                error: passwordValidation.message
            });
        }

        // 检查邮箱是否已注册
        const existingAccount = await Account.findOne({
            where: { email }
        });

        if (existingAccount) {
            return res.status(400).json(ErrorCode.USER_ALREADY_EXIST);
        }

        // 验证验证码
        const validCode = await VerificationCode.findOne({
            where: {
                email,
                code,
                expiresAt: {
                    [Op.gt]: new Date()
                }
            }
        });

        if (!validCode) {
            return res.status(400).json(ErrorCode.CODE_INVALID);
        }

        // 生成盐值并哈希密码
        const salt = generateSalt();
        const hashedPassword = hashPassword(password, salt);

        // 创建账户
        const newAccount = await Account.create({
            email,
            password: hashedPassword,
            salt
        });

        res.json({
            code: 200,
            message: '注册成功',
            data: {
                email: newAccount.email,
                username: newAccount.username,
                avatarUrl: newAccount.avatarUrl
            }
        });

    } catch (error) {
        console.error('注册过程发生错误:', error);
        res.status(500).json(ErrorCode.SERVER_ERROR);
    }
});

module.exports = router; 