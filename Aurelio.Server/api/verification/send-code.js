const express = require('express');
const router = express.Router();
const { sendEmailVerification } = require('../../services/verificationService');
const ErrorCode = require('../../config/errorCode');

router.post('/', async (req, res) => {
    try {
        const { email } = req.body;

        if (!email) {
            return res.status(400).json({
                ...ErrorCode.PARAMS_EMPTY,
                error: '邮箱地址不能为空'
            });
        }

        const sent = await sendEmailVerification(email);

        if (sent) {
            res.json({
                code: 200,
                message: '验证码发送成功'
            });
        } else {
            res.status(500).json(ErrorCode.CODE_SEND_FAILED);
        }
    } catch (error) {
        console.error('发送验证码时发生错误:', error);
        res.status(500).json(ErrorCode.SERVER_ERROR);
    }
});

module.exports = router;
