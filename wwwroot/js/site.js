document.addEventListener('DOMContentLoaded', function () {
    // 自动淡出全局通知提醒（5秒后）
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function (alert) {
        setTimeout(function () {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) {
                bsAlert.close();
            }
        }, 5000);
    });

    // 导航栏当前页面高亮
    const currentPath = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('.navbar-nav .nav-link');
    navLinks.forEach(function (link) {
        const href = link.getAttribute('href');
        if (href) {
            const lowerHref = href.toLowerCase();
            if ((currentPath === '/' || currentPath === '/home' || currentPath === '/home/index') && (lowerHref === '/' || lowerHref === '/home' || lowerHref === '/home/index')) {
                link.classList.add('active');
            } else if (lowerHref !== '/' && lowerHref !== '/home' && lowerHref !== '/home/index' && currentPath.startsWith(lowerHref)) {
                link.classList.add('active');
            }
        }
    });
});
