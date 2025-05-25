const express = require('express');
const apiRoutes = require('./api');
const { syncModels } = require('./models');
const { startCleanupTask } = require('./services/cleanupService');

const app = express();
const port = 3000;

app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// 初始化数据库模型
syncModels().then(() => {
    // 启动验证码清理任务
    startCleanupTask();
});

app.get('/', (req, res) => {
    res.json({
        code: 200,
        message: '服务器运行正常'
    });
});

// Use API routes
app.use('/api', apiRoutes);

app.listen(port, () => {
    console.log(`服务器已启动，运行在 http://localhost:${port}`);
});