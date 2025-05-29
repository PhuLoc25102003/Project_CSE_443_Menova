// wwwroot/js/admin/admin-layout.js
image.png// Hàm thiết lập xác nhận xóa - tách riêng để có thể gọi lại sau khi DOM thay đổi
function setupDeleteConfirmations() {
    console.log('Setting up SweetAlert Delete Confirmation');
    const deleteLinks = document.querySelectorAll('a[href*="Delete"]');
    console.log(`Found ${deleteLinks.length} delete links`, deleteLinks);
    
    // Hàm decode HTML entities
    const decodeHtml = function(html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    };
    
    deleteLinks.forEach(function(link) {
        const href = link.getAttribute('href');
        const isDeleteLink = href.includes('/Delete/') || 
                           href.includes('/Delete?') ||
                           href.includes('Delete');
        
        console.log('Link href:', href, 'Is delete link:', isDeleteLink);
        
        if (isDeleteLink && !link.classList.contains('sweetalert-initialized')) {
            console.log('Initializing SweetAlert for link:', href);
            link.classList.add('sweetalert-initialized');
            
            link.addEventListener('click', function(e) {
                console.log('Delete link clicked:', href);
                e.preventDefault();
                
                const url = this.getAttribute('href');
                const itemType = url.includes('Category') ? 'danh mục' : 'sản phẩm';
                const itemName = this.closest('tr') ? 
                    (this.closest('tr').querySelector('td:nth-child(2)') ? 
                        this.closest('tr').querySelector('td:nth-child(2)').innerText.trim() : '') : '';
                
                const title = `Xác nhận xóa ${itemType}`;
                const text = itemName ? 
                    `Bạn có chắc muốn xóa ${itemType} "${decodeHtml(itemName)}" không?` : 
                    `Bạn có chắc muốn xóa ${itemType} này không?`;
                
                console.log('Showing SweetAlert with:', {title, text});
                
                confirmDelete(decodeHtml(title), decodeHtml(text), 'Xác nhận xóa', 'Hủy').then((result) => {
                    console.log('SweetAlert result:', result);
                    if (result.isConfirmed) {
                        // Send POST request to server
                        const form = document.createElement('form');
                        form.style.display = 'none';
                        form.method = 'post';
                        form.action = url;
                        
                        // Add antiforgery token
                        const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]');
                        console.log('Anti-forgery token found:', antiForgeryToken ? 'Yes' : 'No');
                        if (antiForgeryToken) {
                            const tokenInput = document.createElement('input');
                            tokenInput.type = 'hidden';
                            tokenInput.name = '__RequestVerificationToken';
                            tokenInput.value = antiForgeryToken.value;
                            form.appendChild(tokenInput);
                        }
                        
                        // Add item ID
                        const idMatch = url.match(/\/Delete\/(\d+)/) || 
                                        url.match(/[?&]id=(\d+)/) || 
                                        url.match(/\/Delete\?id=(\d+)/);
                                        
                        console.log('ID match:', idMatch);
                        if (idMatch && idMatch[1]) {
                            const idInput = document.createElement('input');
                            idInput.type = 'hidden';
                            idInput.name = 'id';
                            idInput.value = idMatch[1];
                            form.appendChild(idInput);
                        }
                        
                        document.body.appendChild(form);
                        console.log('Submitting form:', form);
                        form.submit();
                    }
                });
            });
        }
    });
}

// Hàm test xóa trong console để debug
function testDeleteConfirmation() {
    console.log('Testing delete confirmation');
    if (typeof confirmDelete === 'undefined') {
        console.error('confirmDelete function is not defined!');
        return;
    }
    confirmDelete('Test Confirmation', 'This is a test confirmation dialog')
        .then(result => console.log('Test result:', result));
}

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
    
    // Thiết lập xác nhận xóa
    setTimeout(function() {
        console.log('Delayed setup of delete confirmations');
        setupDeleteConfirmations();
    }, 500); // Đợi 500ms để đảm bảo trang đã load hết
    
    // SweetAlert for DeleteVariant confirmation
    document.querySelectorAll('form[action*="DeleteVariant"]').forEach(function(form) {
        if (!form.classList.contains('sweetalert-initialized')) {
            form.classList.add('sweetalert-initialized');
            
            form.addEventListener('submit', function(e) {
                e.preventDefault();
                
                confirmDelete('Xác nhận xóa biến thể', 'Bạn có chắc muốn xóa biến thể này không?')
                    .then((result) => {
                        if (result.isConfirmed) {
                            form.submit();
                        }
                    });
            });
        }
    });

    // Toast notifications for stock updates
    document.querySelectorAll('form[action*="UpdateVariantStock"]').forEach(function(form) {
        if (!form.classList.contains('toast-initialized')) {
            form.classList.add('toast-initialized');
            
            form.addEventListener('submit', function(e) {
                const originalValue = this.querySelector('input[name="StockQuantity"]').defaultValue;
                const newValue = this.querySelector('input[name="StockQuantity"]').value;
                
                // Only show toast if the value actually changed
                if (originalValue !== newValue) {
                    // Update the default value to the new one
                    this.querySelector('input[name="StockQuantity"]').defaultValue = newValue;
                    
                    // Show a toast notification instead of submitting the form if using AJAX
                    // If not using AJAX, you can uncomment the next line
                    // showToast(`Số lượng đã được cập nhật thành ${newValue}`, 'success', 'top-end', 1500);
                }
            });
        }
    });
    
    // Toast notifications for toggle status
    document.querySelectorAll('form[action*="ToggleVariantStatus"], form[action*="ToggleStatus"]').forEach(function(form) {
        if (!form.classList.contains('toast-initialized')) {
            form.classList.add('toast-initialized');
            
            form.addEventListener('submit', function(e) {
                const button = this.querySelector('button');
                const isCurrentlyActive = button.classList.contains('btn-success');
                const newStatus = isCurrentlyActive ? 'ẩn' : 'hiển thị';
                
                // Show a toast notification instead of submitting the form if using AJAX
                // If not using AJAX, you can uncomment the next line
                // showToast(`Trạng thái đã được cập nhật thành ${newStatus}`, 'info', 'top-end', 1500);
            });
        }
    });
    
    console.log('Admin layout script loaded completely');
});