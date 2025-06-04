/**
 * Order Detail Page Functionality
 */

// Initialize page functionality
document.addEventListener('DOMContentLoaded', function() {
    setupPhoneButton();
    
    // Debug logging for order items
    console.log("Debugging order items:");
    const orderItemsContainer = document.querySelector('.order-items');
    const items = document.querySelectorAll('.item');
    console.log("Order items container:", orderItemsContainer);
    console.log("Number of items found:", items.length);
    
    items.forEach((item, index) => {
        console.log(`Item ${index}:`, item);
        console.log(`Item ${index} visibility:`, window.getComputedStyle(item).display);
        console.log(`Item ${index} height:`, window.getComputedStyle(item).height);
        console.log(`Item ${index} children:`, item.children.length);
    });
    
    // Check for empty container with no items
    if (items.length === 0) {
        console.warn("No order items found in container!");
        const cardBody = document.querySelector('.card-body');
        console.log("Card body content:", cardBody?.innerHTML);
    }
    
    // Đánh dấu trạng thái hiện tại trong timeline - cải tiến
    highlightCurrentStatus();

    // Animation for items when page loads
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
    // Không cần làm gì cả vì đã xử lý trong HTML
    // Chỉ để cho debug và hiển thị thông tin
    
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
    
    // Kiểm tra các phần tử timeline hiện tại để debug
    const timelineItems = document.querySelectorAll('.timeline-item');
    timelineItems.forEach((item, index) => {
        const itemStatus = item.getAttribute('data-status');
        const currentClasses = Array.from(item.classList).join(', ');
        console.log(`Timeline item ${index} (${itemStatus}): Classes = ${currentClasses}`);
    });
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