// wwwroot/js/admin/admin-layout.js
document.addEventListener('DOMContentLoaded', function () {
    // Xử lý đóng/mở sidebar
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebarToggleMobile = document.getElementById('sidebar-toggle-mobile');
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');

    // Nếu trên thiết bị desktop
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
        });
    }

    // Nếu trên thiết bị mobile
    if (sidebarToggleMobile) {
        sidebarToggleMobile.addEventListener('click', function () {
            sidebar.classList.toggle('active');
        });
    }

    // Đóng sidebar khi click ra ngoài (chỉ trên mobile)
    document.addEventListener('click', function (event) {
        const isMobile = window.innerWidth <= 992;
        if (isMobile && sidebar.classList.contains('active')) {
            // Kiểm tra nếu click bên ngoài sidebar và không phải là button toggle
            if (!sidebar.contains(event.target) &&
                event.target !== sidebarToggleMobile &&
                !sidebarToggleMobile.contains(event.target)) {
                sidebar.classList.remove('active');
            }
        }
    });

    // Xử lý dropdown
    const dropdowns = document.querySelectorAll('.dropdown');

    dropdowns.forEach(dropdown => {
        const toggleBtn = dropdown.querySelector('.dropdown-toggle');
        const menu = dropdown.querySelector('.dropdown-menu');

        if (toggleBtn && menu) {
            toggleBtn.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                // Đóng tất cả các dropdown khác
                dropdowns.forEach(otherDropdown => {
                    if (otherDropdown !== dropdown) {
                        const otherMenu = otherDropdown.querySelector('.dropdown-menu');
                        if (otherMenu) {
                            otherMenu.style.display = 'none';
                        }
                    }
                });

                // Toggle dropdown hiện tại
                if (menu.style.display === 'block') {
                    menu.style.display = 'none';
                } else {
                    menu.style.display = 'block';
                }
            });
        }
    });

    // Đóng dropdown khi click ra ngoài
    document.addEventListener('click', function (event) {
        dropdowns.forEach(dropdown => {
            const menu = dropdown.querySelector('.dropdown-menu');
            const toggleBtn = dropdown.querySelector('.dropdown-toggle');

            if (menu && menu.style.display === 'block') {
                if (!menu.contains(event.target) &&
                    event.target !== toggleBtn &&
                    !toggleBtn.contains(event.target)) {
                    menu.style.display = 'none';
                }
            }
        });
    });

    // Xử lý responsive
    function handleResize() {
        const isMobile = window.innerWidth <= 992;

        if (isMobile) {
            sidebar.classList.remove('collapsed');
            mainContent.classList.remove('expanded');
        }
    }

    // Initial setup
    handleResize();

    // Resize listener
    window.addEventListener('resize', handleResize);
});