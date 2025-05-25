const VerificationCode = require('../models/verificationCode');
const { Op } = require('sequelize');
const { sendVerificationCode } = require('./emailService');

function generateCode() {
    return Math.floor(100000 + Math.random() * 900000).toString();
}

async function getOrCreateVerificationCode(email) {
    try {
        // Check for existing valid code
        const existingCode = await VerificationCode.findOne({
            where: {
                email,
                expiresAt: {
                    [Op.gt]: new Date()
                }
            }
        });

        if (existingCode) {
            return existingCode.code;
        }

        // Generate new code
        const code = generateCode();
        const expiresAt = new Date(Date.now() + 10 * 60 * 1000); // 10 minutes from now

        // Delete any existing codes for this email
        await VerificationCode.destroy({
            where: { email }
        });

        // Create new verification code
        await VerificationCode.create({
            email,
            code,
            expiresAt
        });

        return code;
    } catch (error) {
        console.error('获取或创建验证码时发生错误:', error);
        throw error;
    }
}

async function sendEmailVerification(email) {
    try {
        const code = await getOrCreateVerificationCode(email);
        const sent = await sendVerificationCode(email, code);
        return sent;
    } catch (error) {
        console.error('发送邮件验证时发生错误:', error);
        return false;
    }
}

module.exports = {
    sendEmailVerification
}; 