/**
 * Order Detail Page Functionality
 */

// Initialize page functionality
document.addEventListener('DOMContentLoaded', function() {
    setupPrintButton();
    setupPhoneButton();
    setupStatusHighlight();

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
});

// Highlight current status in timeline
function setupStatusHighlight() {
    const currentStatus = document.querySelector('.status-badge').innerText.trim().toLowerCase();
    const timelineItems = document.querySelectorAll('.timeline-item');
    
    timelineItems.forEach(item => {
        const statusText = item.querySelector('.timeline-title').innerText.trim().toLowerCase();
        if (statusText.includes(currentStatus)) {
            item.classList.add('current-status');
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