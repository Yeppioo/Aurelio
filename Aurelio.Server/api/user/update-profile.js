const express = require('express');
const router = express.Router();
const { Account } = require('../../models');
const { verifyPassword } = require('../../services/cryptoService');
const ErrorCode = require('../../config/errorCode');

router.post('/', async (req, res) => {
    try {
        const { email, password, username, avatarUrl } = req.body;

        if (!email || !password) {
            return res.status(400).json({
                ...ErrorCode.PARAMS_EMPTY,
                error: '邮箱和密码不能为空'
            });
        }

        if (!username && !avatarUrl) {
            return res.status(400).json({
                code: 400,
                message: '没有提供要更新的信息'
            });
        }

        // 查找用户账户
        const userAccount = await Account.findOne({
            where: { email }
        });

        if (!userAccount) {
            return res.status(400).json(ErrorCode.USER_NOT_EXIST);
        }

        // 验证密码
        const isPasswordValid = verifyPassword(password, userAccount.salt, userAccount.password);

        if (!isPasswordValid) {
            return res.status(400).json(ErrorCode.PASSWORD_INVALID);
        }

        // 准备更新的数据
        const updateData = {};
        if (username) updateData.username = username;
        if (avatarUrl) updateData.avatarUrl = avatarUrl;

        // 更新用户信息
        await userAccount.update(updateData);

        res.json({
            code: 200,
            message: '用户信息更新成功',
            data: {
                email: userAccount.email,
                username: userAccount.username,
                avatarUrl: userAccount.avatarUrl
            }
        });

    } catch (error) {
        console.error('更新用户信息时发生错误:', error);
        res.status(500).json(ErrorCode.SERVER_ERROR);
    }
});

module.exports = router; 