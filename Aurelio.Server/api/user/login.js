const express = require('express');
const router = express.Router();
const { Account } = require('../../models');
const { verifyPassword } = require('../../services/cryptoService');
const ErrorCode = require('../../config/errorCode');

router.post('/', async (req, res) => {
    try {
        const { account, password } = req.body;

        if (!account || !password) {
            return res.status(400).json(ErrorCode.PARAMS_EMPTY);
        }

        // 查找用户账户（通过邮箱查找，区分大小写）
        const userAccount = await Account.findOne({
            where: { email: account }
        });

        if (!userAccount) {
            return res.status(400).json(ErrorCode.USER_NOT_EXIST);
        }

        // 验证密码
        const isPasswordValid = verifyPassword(password, userAccount.salt, userAccount.password);

        if (!isPasswordValid) {
            return res.status(400).json(ErrorCode.PASSWORD_INVALID);
        }

        // 登录成功，返回用户信息
        res.json({
            code: 200,
            message: '登录成功',
            data: {
                email: userAccount.email,
                username: userAccount.username,
                avatarUrl: userAccount.avatarUrl
            }
        });

    } catch (error) {
        console.error('登录过程发生错误:', error);
        res.status(500).json(ErrorCode.SERVER_ERROR);
    }
});

module.exports = router; 