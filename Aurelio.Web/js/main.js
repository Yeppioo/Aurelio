// Main JavaScript functionality for Aurelio website

document.addEventListener('DOMContentLoaded', function () {
    // Initialize all components
    initNavigation();
    initSmoothScrolling();
    initTabShowcase();
    initScrollEffects();
});

// Navigation functionality
function initNavigation() {
    const navToggle = document.querySelector('.nav-toggle');
    const navMenu = document.querySelector('.nav-menu');
    const navLinks = document.querySelectorAll('.nav-link');

    // Mobile menu toggle
    if (navToggle && navMenu) {
        navToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            navToggle.classList.toggle('active');
            navMenu.classList.toggle('active');

            // Add/remove body scroll lock when menu is open
            if (navMenu.classList.contains('active')) {
                document.body.style.overflow = 'hidden';
            } else {
                document.body.style.overflow = '';
            }
        });

        // Close menu when clicking on a link
        navLinks.forEach(link => {
            link.addEventListener('click', function () {
                navToggle.classList.remove('active');
                navMenu.classList.remove('active');
                document.body.style.overflow = '';
            });
        });

        // Close menu when clicking outside
        document.addEventListener('click', function (e) {
            if (!navToggle.contains(e.target) && !navMenu.contains(e.target)) {
                navToggle.classList.remove('active');
                navMenu.classList.remove('active');
                document.body.style.overflow = '';
            }
        });

        // Close menu on escape key
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && navMenu.classList.contains('active')) {
                navToggle.classList.remove('active');
                navMenu.classList.remove('active');
                document.body.style.overflow = '';
            }
        });
    }

    // Header scroll effect
    const header = document.querySelector('.header');
    if (header) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 100) {
                header.style.backgroundColor = 'rgba(255, 255, 255, 0.98)';
                header.style.boxShadow = '0 2px 20px rgba(0, 0, 0, 0.1)';
            } else {
                header.style.backgroundColor = 'rgba(255, 255, 255, 0.95)';
                header.style.boxShadow = 'none';
            }
        });
    }
}

// Smooth scrolling for anchor links
function initSmoothScrolling() {
    const links = document.querySelectorAll('a[href^="#"]');

    links.forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            const targetId = this.getAttribute('href');
            const targetElement = document.querySelector(targetId);

            if (targetElement) {
                const headerHeight = document.querySelector('.header').offsetHeight;
                const targetPosition = targetElement.offsetTop - headerHeight - 20;

                window.scrollTo({
                    top: targetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });
}

// Tab showcase animation
function initTabShowcase() {
    const tabWindow = document.querySelector('.tab-window');
    const tabs = document.querySelectorAll('.tab');
    const tabContent = document.querySelector('.content-area');

    if (!tabWindow || !tabs.length) return;

    // Tab switching animation
    tabs.forEach((tab, index) => {
        tab.addEventListener('click', function () {
            // Remove active class from all tabs
            tabs.forEach(t => t.classList.remove('active'));

            // Add active class to clicked tab
            this.classList.add('active');

            // Animate content change
            if (tabContent) {
                tabContent.style.opacity = '0.5';
                tabContent.style.transform = 'scale(0.95)';

                setTimeout(() => {
                    // Update content based on tab
                    updateTabContent(index);

                    tabContent.style.opacity = '1';
                    tabContent.style.transform = 'scale(1)';
                }, 150);
            }
        });
    });

    // Handle new tab button
    const newTabButton = document.querySelector('.tab-new');
    if (newTabButton) {
        newTabButton.addEventListener('click', function () {
            createNewTab();
        });
    }

    // Handle tab close buttons
    function setupTabCloseHandlers() {
        const tabCloseButtons = document.querySelectorAll('.tab-close');
        tabCloseButtons.forEach((closeBtn, index) => {
            closeBtn.addEventListener('click', function (e) {
                e.stopPropagation(); // Prevent tab selection when clicking close
                closeTab(index);
            });
        });
    }

    // Initial setup of close handlers
    setupTabCloseHandlers();
}

// Update tab content based on selected tab
function updateTabContent(tabIndex) {
    const contentArea = document.querySelector('.content-area');
    if (!contentArea) return;

    const contents = [
        // File Explorer
        `<div class="file-explorer">
            <div class="file-tree">
                <div class="file-item folder">📁 Documents</div>
                <div class="file-item folder">📁 Projects</div>
                <div class="file-item file">📄 README.md</div>
                <div class="file-item file">📄 config.json</div>
                <div class="file-item file">🖼️ screenshot.png</div>
            </div>
        </div>`,

        // Terminal
        `<div class="terminal-demo">
            <div style="font-family: monospace; color: #10b981; font-size: 12px; line-height: 1.4;text-align: left;">
                <div>$ dotnet --version</div>
                <div style="color: #64748b;">8.0.0</div>
                <div>$ dotnet run --project Aurelio.Desktop</div>
                <div style="color: #64748b;">启动 Aurelio 应用程序...</div>
                <div style="color: #64748b;">应用程序已成功启动</div>
                <div>$ _</div>
            </div>
        </div>`,

        // Settings
        `<div class="settings-demo">
            <div style="display: flex; flex-direction: column; gap: 8px; font-size: 12px;">
                <div style="display: flex; justify-content: space-between; align-items: center; padding: 8px; background: rgba(37, 99, 235, 0.1); border-radius: 4px;">
                    <span>🎨 主题</span>
                    <span style="color: #64748b;">深色</span>
                </div>
                <div style="display: flex; justify-content: space-between; align-items: center; padding: 8px; background: rgba(37, 99, 235, 0.1); border-radius: 4px;">
                    <span>🌍 语言</span>
                    <span style="color: #64748b;">中文</span>
                </div>
                <div style="display: flex; justify-content: space-between; align-items: center; padding: 8px; background: rgba(37, 99, 235, 0.1); border-radius: 4px;">
                    <span>🔌 插件</span>
                    <span style="color: #10b981;">已启用</span>
                </div>
            </div>
        </div>`
    ];

    contentArea.innerHTML = contents[tabIndex] || contents[0];
}

// Create new tab functionality
function createNewTab() {
    const tabBar = document.querySelector('.tab-bar');
    const newTabButton = document.querySelector('.tab-new');

    if (!tabBar || !newTabButton) return;

    // Tab types for new tabs
    const newTabTypes = [
        { name: '新标签页', icon: 'M12 4.5v15m7.5-7.5h-15', content: 0 },
        { name: '文档', icon: 'M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z', content: 0 },
        { name: '代码编辑器', icon: 'M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4', content: 1 }
    ];

    // Randomly select a tab type
    const randomType = newTabTypes[Math.floor(Math.random() * newTabTypes.length)];

    // Create new tab element
    const newTab = document.createElement('div');
    newTab.className = 'tab';
    newTab.innerHTML = `
        <svg class="tab-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor">
            <path d="${randomType.icon}"/>
        </svg>
        <span>${randomType.name}</span>
        <div class="tab-close">×</div>
    `;

    // Insert before the new tab button
    tabBar.insertBefore(newTab, newTabButton);

    // Add click handler for the new tab
    newTab.addEventListener('click', function () {
        // Remove active class from all tabs
        const allTabs = document.querySelectorAll('.tab');
        allTabs.forEach(t => t.classList.remove('active'));

        // Add active class to clicked tab
        this.classList.add('active');

        // Update content
        const tabIndex = Array.from(allTabs).indexOf(this);
        updateTabContent(randomType.content);
    });

    // Setup close handler for the new tab
    const closeBtn = newTab.querySelector('.tab-close');
    closeBtn.addEventListener('click', function (e) {
        e.stopPropagation();
        const tabIndex = Array.from(document.querySelectorAll('.tab')).indexOf(newTab);
        closeTab(tabIndex);
    });

    // Automatically select the new tab
    const allTabs = document.querySelectorAll('.tab');
    allTabs.forEach(t => t.classList.remove('active'));
    newTab.classList.add('active');
    updateTabContent(randomType.content);

    // Add creation animation
    newTab.classList.add('tab-creating');
    setTimeout(() => {
        newTab.classList.remove('tab-creating');
    }, 300);

    // Scroll to the new tab
    scrollToTab(newTab);


}

// Close tab functionality
function closeTab(tabIndex) {
    const tabs = document.querySelectorAll('.tab');
    const tabToClose = tabs[tabIndex];

    if (!tabToClose || tabs.length <= 1) {
        // Don't close if it's the last tab
        return;
    }

    const wasActive = tabToClose.classList.contains('active');

    // Add closing animation
    tabToClose.classList.add('tab-closing');

    setTimeout(() => {
        // Remove the tab
        tabToClose.remove();

        // If the closed tab was active, select another tab
        if (wasActive) {
            const remainingTabs = document.querySelectorAll('.tab');
            if (remainingTabs.length > 0) {
                // Select the previous tab, or the first one if we closed the first tab
                const newActiveIndex = Math.min(tabIndex, remainingTabs.length - 1);
                remainingTabs[newActiveIndex].classList.add('active');

                // Update content for the newly active tab
                updateTabContent(newActiveIndex % 3); // Cycle through content types
            }
        }
    }, 300);


}

// Scroll to specific tab in the tab bar
function scrollToTab(tab) {
    const tabBar = document.querySelector('.tab-bar');
    if (!tabBar || !tab) return;

    const tabBarRect = tabBar.getBoundingClientRect();
    const tabRect = tab.getBoundingClientRect();

    // Calculate the scroll position to center the tab
    const scrollLeft = tab.offsetLeft - (tabBarRect.width / 2) + (tabRect.width / 2);

    tabBar.scrollTo({
        left: scrollLeft,
        behavior: 'smooth'
    });
}

// Scroll effects for elements
function initScrollEffects() {
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

    // Observe feature cards
    const featureCards = document.querySelectorAll('.feature-card');
    featureCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(30px)';
        card.style.transition = `opacity 0.6s ease ${index * 0.1}s, transform 0.6s ease ${index * 0.1}s`;
        observer.observe(card);
    });

    // Observe download cards
    const downloadCards = document.querySelectorAll('.download-card');
    downloadCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(30px)';
        card.style.transition = `opacity 0.6s ease ${index * 0.1}s, transform 0.6s ease ${index * 0.1}s`;
        observer.observe(card);
    });

    // Observe tech categories
    const techCategories = document.querySelectorAll('.tech-category');
    techCategories.forEach((category, index) => {
        category.style.opacity = '0';
        category.style.transform = 'translateY(30px)';
        category.style.transition = `opacity 0.6s ease ${index * 0.1}s, transform 0.6s ease ${index * 0.1}s`;
        observer.observe(category);
    });
}



// Utility functions
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

// Add some interactive effects
document.addEventListener('DOMContentLoaded', function () {
    // Add hover effects to buttons
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-2px)';
        });

        button.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });

    // Add click ripple effect
    buttons.forEach(button => {
        button.addEventListener('click', function (e) {
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;

            ripple.style.cssText = `
                position: absolute;
                width: ${size}px;
                height: ${size}px;
                left: ${x}px;
                top: ${y}px;
                background: rgba(255, 255, 255, 0.3);
                border-radius: 50%;
                transform: scale(0);
                animation: ripple 0.6s ease-out;
                pointer-events: none;
            `;

            this.style.position = 'relative';
            this.style.overflow = 'hidden';
            this.appendChild(ripple);

            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });
});

// Add CSS for ripple animation
const style = document.createElement('style');
style.textContent = `
    @keyframes ripple {
        to {
            transform: scale(2);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);
