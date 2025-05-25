const nodemailer = require('nodemailer');

const transporter = nodemailer.createTransport({
    host: 'smtp.zoho.com',
    port: 465,
    secure: true,
    auth: {
        user: 'aurelio@yep.vin',
        pass: 'nuj4pR_p'
    }
});

async function sendVerificationCode(email, code) {
    const currentDate = new Date().toLocaleString('zh-CN', { timeZone: 'Asia/Shanghai' });

    try {
        await transporter.sendMail({
            from: '"Aurelio" <aurelio@yep.vin>',
            to: email,
            subject: 'Aurelio - 邮箱验证码',
            text: `您的验证码是：${code}。该验证码将在10分钟后过期，请尽快完成验证。`,
            html: `
                <div style="max-width: 600px; margin: 0 auto; padding: 20px; font-family: Arial, sans-serif;">
                    <div style="background: #f8f9fa; padding: 20px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);">
                        <div style="text-align: center; margin-bottom: 20px;">
                            <h1 style="color: #333; margin: 0; font-size: 24px;">邮箱验证码</h1>
                        </div>
                        
                        <div style="background: white; padding: 20px; border-radius: 5px; margin-bottom: 20px;">
                            <p style="color: #666; font-size: 14px; margin: 0 0 10px;">尊敬的用户：</p>
                            <p style="color: #666; font-size: 14px; line-height: 1.5;">您正在进行邮箱验证，本次请求的验证码为：</p>
                            <div style="margin: 20px 0; text-align: center;">
                                <span style="font-size: 32px; font-weight: bold; color: #007bff; letter-spacing: 5px; padding: 10px 20px; background: #f8f9fa; border-radius: 5px;">${code}</span>
                            </div>
                            <p style="color: #666; font-size: 14px; line-height: 1.5;">
                                验证码将在<span style="color: #dc3545; font-weight: bold;">10分钟</span>后过期，请尽快完成验证。
                                <br>如非本人操作，请忽略此邮件。
                            </p>
                        </div>
                        
                        <div style="border-top: 1px solid #eee; padding-top: 20px; margin-top: 20px;">
                            <p style="color: #999; font-size: 12px; margin: 0; text-align: center;">
                                这是一封自动生成的邮件，请勿直接回复
                                <br>
                                发送时间：${currentDate}
                            </p>
                        </div>
                    </div>
                </div>
            `
        });
        return true;
    } catch (error) {
        console.error('发送邮件时发生错误:', error);
        return false;
    }
}

module.exports = {
    sendVerificationCode
}; 