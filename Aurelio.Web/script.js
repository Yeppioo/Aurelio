// 平滑滚动
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});

// 导航栏滚动效果
let lastScrollTop = 0;
const navbar = document.querySelector('.navbar');

window.addEventListener('scroll', () => {
    const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

    // 添加背景模糊效果
    if (scrollTop > 50) {
        navbar.style.background = 'rgba(10, 10, 11, 0.95)';
    } else {
        navbar.style.background = 'rgba(10, 10, 11, 0.8)';
    }

    lastScrollTop = scrollTop;
});

// 页面加载动画
window.addEventListener('load', () => {
    // 添加页面加载完成的类
    document.body.classList.add('loaded');

    // 为元素添加进入动画
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
            }
        });
    }, observerOptions);

    // 观察需要动画的元素
    document.querySelectorAll('.feature-card, .download-card, .hero-content, .hero-visual').forEach(el => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(30px)';
        el.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(el);
    });
});

// 鼠标跟随效果（可选）
document.addEventListener('mousemove', (e) => {
    const cursor = document.querySelector('.cursor');
    if (cursor) {
        cursor.style.left = e.clientX + 'px';
        cursor.style.top = e.clientY + 'px';
    }
});

// 标签页交互效果
document.addEventListener('DOMContentLoaded', () => {
    // 模拟标签页切换
    const tabs = document.querySelectorAll('.tab-item');
    tabs.forEach(tab => {
        tab.addEventListener('click', function () {
            // 移除所有活动状态
            tabs.forEach(t => t.classList.remove('active'));
            // 添加当前活动状态
            this.classList.add('active');
        });
    });

    // 标签页关闭按钮
    document.querySelectorAll('.tab-close').forEach(closeBtn => {
        closeBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            const tab = this.closest('.tab-item');
            tab.style.opacity = '0';
            tab.style.transform = 'translateX(-20px)';
            setTimeout(() => {
                tab.remove();
            }, 200);
        });
    });

    // 新增标签页按钮
    document.querySelector('.tab-add')?.addEventListener('click', function () {
        const tabsContainer = document.querySelector('.mockup-tabs');
        const newTab = document.createElement('div');
        newTab.className = 'tab-item';
        newTab.innerHTML = `
            <span class="tab-icon">🆕</span>
            <span class="tab-title">新实例</span>
            <span class="tab-close">×</span>
        `;

        // 插入到添加按钮前
        tabsContainer.insertBefore(newTab, this);

        // 添加关闭事件
        newTab.querySelector('.tab-close').addEventListener('click', function (e) {
            e.stopPropagation();
            newTab.style.opacity = '0';
            newTab.style.transform = 'translateX(-20px)';
            setTimeout(() => {
                newTab.remove();
            }, 200);
        });

        // 添加点击切换事件
        newTab.addEventListener('click', function () {
            tabs.forEach(t => t.classList.remove('active'));
            this.classList.add('active');
        });

        // 动画效果
        newTab.style.opacity = '0';
        newTab.style.transform = 'translateX(20px)';
        setTimeout(() => {
            newTab.style.opacity = '1';
            newTab.style.transform = 'translateX(0)';
        }, 50);
    });
});

// 添加一些交互效果
document.querySelectorAll('.btn-primary, .btn-secondary').forEach(button => {
    button.addEventListener('mouseenter', function () {
        this.style.transform = 'translateY(-2px)';
    });

    button.addEventListener('mouseleave', function () {
        this.style.transform = 'translateY(0)';
    });
});

// 下载功能
const DOWNLOAD_BASE_URL = 'https://github.com/Yeppioo/Aurelio/releases/download/auto-publish/';
const GITHUB_API_URL = 'https://api.github.com/repos/Yeppioo/Aurelio/releases?per_page=1';

function downloadFile(filename) {
    if (!filename) return;

    const downloadUrl = DOWNLOAD_BASE_URL + filename;

    // 创建下载提示
    showDownloadMessage(`正在下载 ${filename}...`);

    // 创建隐藏的下载链接
    const link = document.createElement('a');
    link.href = downloadUrl;
    link.download = filename;
    link.style.display = 'none';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

function downloadSelected(button) {
    // 找到同一个下载卡片中的选择框
    const downloadCard = button.closest('.download-card');
    const selectElement = downloadCard.querySelector('.download-select');
    const selectedFilename = selectElement.value;

    if (selectedFilename) {
        downloadFile(selectedFilename);
    }
}

function showDownloadMessage(message) {
    // 创建提示消息
    const messageDiv = document.createElement('div');
    messageDiv.textContent = message;
    messageDiv.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: var(--primary-color);
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 0.5rem;
        z-index: 10000;
        font-weight: 500;
        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
        animation: slideInRight 0.3s ease-out;
        max-width: 300px;
        font-size: 0.9rem;
    `;

    document.body.appendChild(messageDiv);

    // 3秒后移除消息
    setTimeout(() => {
        if (messageDiv.parentNode) {
            messageDiv.style.animation = 'slideOutRight 0.3s ease-in';
            setTimeout(() => {
                if (messageDiv.parentNode) {
                    messageDiv.remove();
                }
            }, 300);
        }
    }, 3000);
}

// 添加键盘导航支持
document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') {
        // 关闭任何打开的模态框或菜单
        document.querySelectorAll('.modal, .dropdown').forEach(el => {
            el.style.display = 'none';
        });
    }
});

// 性能优化：防抖函数
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// 优化滚动事件
const optimizedScrollHandler = debounce(() => {
    // 滚动相关的处理逻辑
}, 16); // 约60fps

window.addEventListener('scroll', optimizedScrollHandler);

// 添加主题切换功能（预留）
function toggleTheme() {
    document.body.classList.toggle('light-theme');
    localStorage.setItem('theme', document.body.classList.contains('light-theme') ? 'light' : 'dark');
}

// 恢复用户主题偏好
if (localStorage.getItem('theme') === 'light') {
    document.body.classList.add('light-theme');
}

// 添加一些视觉效果
document.querySelectorAll('.feature-card').forEach(card => {
    card.addEventListener('mouseenter', function () {
        this.style.boxShadow = '0 20px 40px rgba(147, 115, 238, 0.1)';
    });

    card.addEventListener('mouseleave', function () {
        this.style.boxShadow = '';
    });
});

// 添加粒子效果背景（可选）
function createParticles() {
    const particlesContainer = document.createElement('div');
    particlesContainer.className = 'particles';
    particlesContainer.style.cssText = `
        position: fixed;
        top: 0;
        left: 0;
        width: 100%;
        height: 100%;
        pointer-events: none;
        z-index: -1;
    `;

    for (let i = 0; i < 50; i++) {
        const particle = document.createElement('div');
        particle.style.cssText = `
            position: absolute;
            width: 2px;
            height: 2px;
            background: rgba(147, 115, 238, 0.3);
            border-radius: 50%;
            animation: float ${Math.random() * 3 + 2}s ease-in-out infinite;
            left: ${Math.random() * 100}%;
            top: ${Math.random() * 100}%;
            animation-delay: ${Math.random() * 2}s;
        `;
        particlesContainer.appendChild(particle);
    }

    document.body.appendChild(particlesContainer);
}

// 添加浮动动画的CSS
const style = document.createElement('style');
style.textContent = `
    @keyframes float {
        0%, 100% { transform: translateY(0px); opacity: 0.3; }
        50% { transform: translateY(-20px); opacity: 0.8; }
    }
    
    .loaded .hero-content {
        animation: slideInLeft 0.8s ease-out;
    }
    
    .loaded .hero-visual {
        animation: slideInRight 0.8s ease-out;
    }
    
    @keyframes slideInLeft {
        from { opacity: 0; transform: translateX(-50px); }
        to { opacity: 1; transform: translateX(0); }
    }
    
    @keyframes slideInRight {
        from { opacity: 0; transform: translateX(50px); }
        to { opacity: 1; transform: translateX(0); }
    }
`;
document.head.appendChild(style);

// 初始化粒子效果
// createParticles(); // 取消注释以启用粒子效果

// 获取最新版本信息
async function fetchLatestVersion() {
    try {
        const response = await fetch(GITHUB_API_URL);
        const releases = await response.json();

        if (releases && releases.length > 0) {
            const latestRelease = releases[0];
            const version = latestRelease.name || 'v1.0.0';

            // 更新页面中的版本信息
            updateVersionInfo(version);

            return version;
        }
    } catch (error) {
        console.warn('获取版本信息失败，使用默认版本:', error);
        return 'v1.0.0';
    }
}

function updateVersionInfo(version) {
    // 更新下载区域的版本信息
    const downloadInfo = document.querySelector('.version-info');
    if (downloadInfo) {
        downloadInfo.textContent = `当前版本：${version} | 文件大小：~50MB | 免费开源`;
    }

    // 更新关于区域的版本信息
    const aboutVersion = document.querySelector('.version-number');
    if (aboutVersion) {
        aboutVersion.textContent = version.replace('v', '');
    }
}

// 页面加载时获取版本信息
document.addEventListener('DOMContentLoaded', () => {
    fetchLatestVersion();
});

console.log('Aurelio 官网已加载完成！');
