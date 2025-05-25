const express = require('express');
const router = express.Router();
const { Account, VerificationCode } = require('../../models');
const { Op } = require('sequelize');
const ErrorCode = require('../../config/errorCode');

router.post('/', async (req, res) => {
    try {
        const { currentEmail, newEmail, code } = req.body;

        if (!currentEmail || !newEmail || !code) {
            return res.status(400).json({
                ...ErrorCode.PARAMS_EMPTY,
                error: '当前邮箱、新邮箱和验证码不能为空'
            });
        }

        // 检查新邮箱格式
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(newEmail)) {
            return res.status(400).json({
                code: 400,
                message: '新邮箱格式不正确'
            });
        }

        // 查找当前用户账户
        const userAccount = await Account.findOne({
            where: { email: currentEmail }
        });

        if (!userAccount) {
            return res.status(400).json(ErrorCode.USER_NOT_EXIST);
        }

        // 检查新邮箱是否已被使用
        const existingAccount = await Account.findOne({
            where: { email: newEmail }
        });

        if (existingAccount) {
            return res.status(400).json(ErrorCode.USER_ALREADY_EXIST);
        }

        // 验证验证码
        const validCode = await VerificationCode.findOne({
            where: {
                email: newEmail,
                code,
                expiresAt: {
                    [Op.gt]: new Date()
                }
            }
        });

        if (!validCode) {
            return res.status(400).json(ErrorCode.CODE_INVALID);
        }

        // 更新邮箱
        await userAccount.update({
            email: newEmail
        });

        // 删除验证码
        await validCode.destroy();

        res.json({
            code: 200,
            message: '邮箱修改成功',
            data: {
                email: newEmail,
                username: userAccount.username,
                avatarUrl: userAccount.avatarUrl
            }
        });

    } catch (error) {
        console.error('修改邮箱过程发生错误:', error);
        res.status(500).json(ErrorCode.SERVER_ERROR);
    }
});

module.exports = router; 