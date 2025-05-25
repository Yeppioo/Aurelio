/**
 * 错误码对照表
 * 
 * 格式说明：
 * 10xxx - 通用错误
 * 11xxx - 用户相关错误
 * 12xxx - 验证码相关错误
 * 13xxx - 密码相关错误
 * 50xxx - 服务器错误
 */

const ErrorCode = {
    // 通用错误 (10xxx)
    PARAMS_EMPTY: {
        code: 10001,
        message: '必填参数不能为空'
    },

    // 用户相关错误 (11xxx)
    USER_NOT_EXIST: {
        code: 11001,
        message: '账户不存在'
    },
    USER_ALREADY_EXIST: {
        code: 11002,
        message: '该邮箱已被注册'
    },

    // 验证码相关错误 (12xxx)
    CODE_INVALID: {
        code: 12001,
        message: '验证码无效或已过期'
    },
    CODE_SEND_FAILED: {
        code: 12002,
        message: '验证码发送失败'
    },

    // 密码相关错误 (13xxx)
    PASSWORD_INVALID: {
        code: 13001,
        message: '密码错误'
    },
    PASSWORD_FORMAT_INVALID: {
        code: 13002,
        message: '密码不能包含空格或不可见字符'
    },
    PASSWORD_LENGTH_INVALID: {
        code: 13003,
        message: '密码长度必须在6-20位之间'
    },
    PASSWORD_CHARS_INVALID: {
        code: 13004,
        message: '密码只能包含字母、数字和常用特殊字符'
    },

    // 服务器错误 (50xxx)
    SERVER_ERROR: {
        code: 50001,
        message: '服务器内部错误'
    }
};

module.exports = ErrorCode; 