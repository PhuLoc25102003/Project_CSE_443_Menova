/**
 * Order Detail Page Functionality
 */

// Initialize page functionality
document.addEventListener('DOMContentLoaded', function() {
    setupPrintButton();
    setupPhoneButton();
    
    // Đánh dấu trạng thái hiện tại trong timeline - cải tiến
    highlightCurrentStatus();

    // Animation for items when page loads
    const items = document.querySelectorAll('.item');
    items.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateY(20px)';
        setTimeout(() => {
            item.style.transition = 'all 0.6s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateY(0)';
        }, index * 200);
    });

    // Status change functionality
    const statusSelect = document.getElementById('orderStatus');
    if (statusSelect) {
        const currentStatus = statusSelect.getAttribute('data-current-status');
        if (currentStatus) {
            Array.from(statusSelect.options).forEach(option => {
                if (option.value.toLowerCase() === currentStatus.toLowerCase()) {
                    option.selected = true;
                }
            });
        }

        // Handle status change
        statusSelect.addEventListener('change', function() {
            const form = document.getElementById('updateStatusForm');
            if (form && confirm('Bạn có chắc chắn muốn cập nhật trạng thái đơn hàng?')) {
                form.submit();
            }
        });
    }
    
    // Prevent accidental page navigation
    setupFormValidation();
});

// Highlight current status in timeline - cải tiến để làm đúng theo yêu cầu màu sắc
function highlightCurrentStatus() {
    // Lấy trạng thái hiện tại từ badge
    const statusBadge = document.querySelector('.status-badge');
    if (!statusBadge) return;
    
    const currentStatusText = statusBadge.innerText.trim().toLowerCase();
    console.log("Current status text:", currentStatusText);
    
    // Tìm data-status tương ứng với trạng thái hiện tại
    let currentStatus = '';
    if (currentStatusText.includes("chờ xử lý")) currentStatus = "pending";
    else if (currentStatusText.includes("đang xử lý") || currentStatusText.includes("đang chuẩn bị")) currentStatus = "processing";
    else if (currentStatusText.includes("đang giao")) currentStatus = "shipping";
    else if (currentStatusText.includes("đã giao")) currentStatus = "delivered";
    else if (currentStatusText.includes("đã nhận")) currentStatus = "received";
    else if (currentStatusText.includes("đã hủy")) currentStatus = "cancelled";
    
    console.log("Mapped current status:", currentStatus);
    
    // Lấy tất cả các item trong timeline
    const timelineItems = document.querySelectorAll('.timeline-item');
    
    // Status map theo thứ tự trạng thái
    const statusMap = {
        "pending": 0,
        "processing": 1,
        "shipping": 2,
        "delivered": 3,
        "received": 4
    };
    
    // Xác định index của trạng thái hiện tại
    const currentIndex = statusMap[currentStatus] || -1;
    console.log("Current index:", currentIndex);
    
    // Áp dụng class đúng cho từng item trong timeline
    timelineItems.forEach((item, index) => {
        // Reset các class trước khi thêm class mới
        item.classList.remove('timeline-item--active', 'current-status', 'timeline-item--disabled');
        
        // Lấy data-status của item
        const itemStatus = item.getAttribute('data-status');
        const itemIndex = statusMap[itemStatus] || -1;
        
        if (itemStatus === currentStatus) {
            // Nếu là trạng thái hiện tại - màu xanh dương
            item.classList.add('current-status');
            console.log(`Item ${index} (${itemStatus}): Added current-status - THIS IS CURRENT`);
        } else if (itemIndex < currentIndex) {
            // Trạng thái đã qua - màu xanh lá
            item.classList.add('timeline-item--active');
            console.log(`Item ${index} (${itemStatus}): Added timeline-item--active - COMPLETED`);
        } else {
            // Trạng thái tương lai - màu trắng
            item.classList.add('timeline-item--disabled');
            console.log(`Item ${index} (${itemStatus}): Added timeline-item--disabled - FUTURE`);
        }
    });
}

// Print order details
function setupPrintButton() {
    const printButton = document.querySelector('button[onclick="printOrder()"]');
    if (printButton) {
        printButton.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Hide elements not needed in print
            const elementsToHide = document.querySelectorAll('.status-action-buttons, .action-buttons, .floating-actions');
            elementsToHide.forEach(el => {
                el.dataset.initialDisplay = el.style.display;
                el.style.display = 'none';
            });
            
            // Print the window
            window.print();
            
            // Restore hidden elements
            setTimeout(() => {
                elementsToHide.forEach(el => {
                    el.style.display = el.dataset.initialDisplay || '';
                });
            }, 1000);
        });
    }
}

// Handle phone button
function setupPhoneButton() {
    const phoneButtons = document.querySelectorAll('.call-button');
    phoneButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            const phoneNumber = this.closest('.info-row').querySelector('.phone-number').innerText.trim();
            contactCustomer(phoneNumber);
        });
    });
}

// Enhanced status change confirmation
function confirmStatusChange(title, message, formId) {
    if (typeof confirmAction !== 'function') {
        console.warn('confirmAction function not found, falling back to standard confirm');
        if (confirm(message)) {
            document.getElementById('form-' + formId).submit();
        }
        return;
    }
    
    // Use SweetAlert2 confirmation
    confirmAction(title, message, 'Xác nhận', 'Hủy')
        .then((result) => {
            if (result.isConfirmed) {
                // Add loading state to button
                const button = document.querySelector(`#form-${formId} button`);
                if (button) {
                    const originalHtml = button.innerHTML;
                    button.disabled = true;
                    button.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
                    
                    // Submit the form after visual feedback
                    setTimeout(() => {
                        document.getElementById('form-' + formId).submit();
                    }, 500);
                } else {
                    document.getElementById('form-' + formId).submit();
                }
            }
        });
}

// Contact customer function
function contactCustomer(phoneNumber) {
    if (typeof showInfo !== 'function') {
        alert('Liên hệ khách hàng: ' + phoneNumber);
        return;
    }
    
    showInfo(`Bạn sẽ gọi đến số ${phoneNumber}`);
    
    // Optional: implement actual phone call functionality here
    // Example: window.location.href = `tel:${phoneNumber}`;
}

// Print order function
function printOrder() {
    // This function is now handled by setupPrintButton
    window.print();
}

// Form validation to prevent accidental submissions
function setupFormValidation() {
    const statusForms = document.querySelectorAll('form[action*="UpdateStatus"]');
    
    statusForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const confirmed = confirm('Bạn có chắc chắn muốn cập nhật trạng thái đơn hàng?');
            if (!confirmed) {
                e.preventDefault();
                return false;
            }
        });
    });
} 