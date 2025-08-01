// Download functionality for Aurelio website

document.addEventListener('DOMContentLoaded', function () {
    initDownloads();
});

// Download configuration based on the README.md information
const downloadConfig = {
    baseUrl: 'https://github.com/Yeppioo/Aurelio/releases/download/auto-publish/',
    files: {
        // Windows
        'win-x64-installer': 'Aurelio.win.x64.installer.exe',
        'win-x64-portable': 'Aurelio.win.x64.executable.zip',
        'win-x86-installer': 'Aurelio.win.x86.installer.exe',
        'win-x86-portable': 'Aurelio.win.x86.executable.zip',
        'win-arm64-installer': 'Aurelio.win.arm64.installer.exe',
        'win-arm64-portable': 'Aurelio.win.arm64.executable.zip',

        // macOS
        'osx-x64-dmg': 'Aurelio.osx.mac.x64.dmg',
        'osx-x64-app': 'Aurelio.osx.mac.x64.app.zip',
        'osx-arm64-dmg': 'Aurelio.osx.mac.arm64.dmg',
        'osx-arm64-app': 'Aurelio.osx.mac.arm64.app.zip',

        // Linux
        'linux-x64-appimage': 'Aurelio.linux.x64.AppImage',
        'linux-arm64-appimage': 'Aurelio.linux.arm64.AppImage',
        'linux-arm-appimage': 'Aurelio.linux.arm.AppImage'
    }
};

function initDownloads() {
    const downloadButtons = document.querySelectorAll('.btn-download');
    const downloadSelects = document.querySelectorAll('.download-select');

    // Initialize download buttons
    downloadButtons.forEach(button => {
        button.addEventListener('click', handleDownload);
    });

    // Initialize download selects
    downloadSelects.forEach(select => {
        select.addEventListener('change', updateDownloadButton);
    });

    // Set default selections based on user's platform
    detectAndSetDefaultPlatform();
}

function detectAndSetDefaultPlatform() {
    const userAgent = navigator.userAgent.toLowerCase();
    const platform = navigator.platform.toLowerCase();

    let detectedPlatform = 'windows';
    let defaultOption = 'win-x64-installer';

    // Detect platform
    if (userAgent.includes('mac') || platform.includes('mac')) {
        detectedPlatform = 'macos';
        // Detect Apple Silicon vs Intel
        if (userAgent.includes('arm') || platform.includes('arm')) {
            defaultOption = 'osx-arm64-dmg';
        } else {
            defaultOption = 'osx-x64-dmg';
        }
    } else if (userAgent.includes('linux') || platform.includes('linux')) {
        detectedPlatform = 'linux';
        // Detect architecture
        if (userAgent.includes('arm64') || platform.includes('arm64')) {
            defaultOption = 'linux-arm64-appimage';
        } else if (userAgent.includes('arm')) {
            defaultOption = 'linux-arm-appimage';
        } else {
            defaultOption = 'linux-x64-appimage';
        }
    } else if (userAgent.includes('win') || platform.includes('win')) {
        detectedPlatform = 'windows';
        // Detect architecture
        if (userAgent.includes('wow64') || userAgent.includes('win64') || userAgent.includes('x64')) {
            defaultOption = 'win-x64-installer';
        } else if (userAgent.includes('arm')) {
            defaultOption = 'win-arm64-installer';
        } else {
            defaultOption = 'win-x86-installer';
        }
    }

    // Set default selection
    const platformSelect = document.querySelector(`[data-platform="${detectedPlatform}"]`);
    if (platformSelect && platformSelect.tagName === 'SELECT') {
        platformSelect.value = defaultOption;
        updateDownloadButton({ target: platformSelect });
    }

}

function updateDownloadButton(event) {
    const select = event.target;
    const platform = select.dataset.platform;
    const selectedValue = select.value;

    const button = select.parentElement.querySelector('.btn-download');
    if (button) {
        button.dataset.downloadType = selectedValue;

        // Update button text based on selection
        const buttonText = getButtonText(selectedValue);
        const iconSvg = button.querySelector('svg').outerHTML;
        button.innerHTML = iconSvg + buttonText;
    }
}

function getButtonText(downloadType) {
    const textMap = {
        // Windows
        'win-x64-installer': '下载安装包',
        'win-x64-portable': '下载便携版',
        'win-x86-installer': '下载安装包',
        'win-x86-portable': '下载便携版',
        'win-arm64-installer': '下载安装包',
        'win-arm64-portable': '下载便携版',

        // macOS
        'osx-x64-dmg': '下载 DMG',
        'osx-x64-app': '下载 APP',
        'osx-arm64-dmg': '下载 DMG',
        'osx-arm64-app': '下载 APP',

        // Linux
        'linux-x64-appimage': '下载 AppImage',
        'linux-arm64-appimage': '下载 AppImage',
        'linux-arm-appimage': '下载 AppImage'
    };

    return textMap[downloadType] || '下载';
}

function handleDownload(event) {
    event.preventDefault();

    const button = event.currentTarget;
    const platform = button.dataset.platform;

    // Get selected download type
    let downloadType = button.dataset.downloadType;

    // If no download type is set, get it from the select
    if (!downloadType) {
        const select = button.parentElement.querySelector('.download-select');
        downloadType = select ? select.value : getDefaultDownloadType(platform);
    }

    // Get download URL
    const downloadUrl = getDownloadUrl(downloadType);

    if (downloadUrl) {
        // Add download animation
        animateDownloadButton(button);

        // Start download
        setTimeout(() => {
            window.open(downloadUrl, '_blank');

            // Track download (you can add analytics here)
            trackDownload(downloadType);
        }, 500);
    }
}

function getDefaultDownloadType(platform) {
    const defaults = {
        'windows': 'win-x64-installer',
        'macos': 'osx-x64-dmg',
        'linux': 'linux-x64-appimage'
    };

    return defaults[platform] || 'win-x64-installer';
}

function getDownloadUrl(downloadType) {
    const fileName = downloadConfig.files[downloadType];
    if (fileName) {
        return downloadConfig.baseUrl + fileName;
    }
    return null;
}

function animateDownloadButton(button) {
    const originalContent = button.innerHTML;

    // Change button to loading state
    button.innerHTML = `
        <svg class="btn-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <circle cx="12" cy="12" r="10"/>
            <path d="M12 6v6l4 2"/>
        </svg>
        下载中...
    `;

    button.style.opacity = '0.7';
    button.style.pointerEvents = 'none';

    // Reset button after animation
    setTimeout(() => {
        button.innerHTML = originalContent;
        button.style.opacity = '1';
        button.style.pointerEvents = 'auto';
    }, 2000);
}

function trackDownload(downloadType) {
    // You can add analytics tracking here
    console.log(`Download started: ${downloadType}`);

    // Example: Google Analytics tracking
    if (typeof gtag !== 'undefined') {
        gtag('event', 'download', {
            'event_category': 'software',
            'event_label': downloadType,
            'value': 1
        });
    }

    // Example: Custom analytics
    if (typeof analytics !== 'undefined') {
        analytics.track('Download Started', {
            downloadType: downloadType,
            platform: downloadType.split('-')[0],
            timestamp: new Date().toISOString()
        });
    }
}

// Add keyboard shortcuts for quick downloads
document.addEventListener('keydown', function (event) {
    // Ctrl/Cmd + D for quick download
    if ((event.ctrlKey || event.metaKey) && event.key === 'd') {
        event.preventDefault();

        const firstDownloadButton = document.querySelector('.btn-download');
        if (firstDownloadButton) {
            firstDownloadButton.click();
        }
    }
});

// Add download progress simulation (for demo purposes)
function simulateDownloadProgress() {
    // This is just for demonstration - real downloads would be handled by the browser
    return new Promise((resolve) => {
        let progress = 0;
        const interval = setInterval(() => {
            progress += Math.random() * 20;
            if (progress >= 100) {
                progress = 100;
                clearInterval(interval);
                resolve();
            }
        }, 200);
    });
}

// Export functions for potential use by other scripts
window.AurelioDownloads = {
    handleDownload,
    getDownloadUrl,
    trackDownload,
    detectAndSetDefaultPlatform
};
