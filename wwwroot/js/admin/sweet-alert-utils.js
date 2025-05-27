/**
 * SweetAlert Utilities for Admin Panel
 */

// Hàm giải mã HTML entities
function decodeHtml(html) {
    if (!html) return '';
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
}

// Function to show success notification
function showSuccess(message, timer = 2000) {
    console.log('showSuccess called with:', message);
    message = decodeHtml(message);
    
    if (typeof Swal === 'undefined') {
        console.error('SweetAlert2 is not available! Cannot show success notification');
        alert('Thành công: ' + message);
        return;
    }
    
    Swal.fire({
        title: 'Thành công!',
        text: message,
        icon: 'success',
        confirmButtonText: 'Đồng ý',
        timer: timer,
        timerProgressBar: true
    });
}

// Function to show error notification
function showError(message) {
    console.log('showError called with:', message);
    message = decodeHtml(message);
    
    if (typeof Swal === 'undefined') {
        console.error('SweetAlert2 is not available! Cannot show error notification');
        alert('Lỗi: ' + message);
        return;
    }
    
    Swal.fire({
        title: 'Lỗi!',
        text: message,
        icon: 'error',
        confirmButtonText: 'Đồng ý'
    });
}

// Function to show warning notification
function showWarning(message) {
    console.log('showWarning called with:', message);
    message = decodeHtml(message);
    
    if (typeof Swal === 'undefined') {
        console.error('SweetAlert2 is not available! Cannot show warning notification');
        alert('Cảnh báo: ' + message);
        return;
    }
    
    Swal.fire({
        title: 'Cảnh báo!',
        text: message,
        icon: 'warning',
        confirmButtonText: 'Đồng ý'
    });
}

// Function to show info notification
function showInfo(message) {
    console.log('showInfo called with:', message);
    message = decodeHtml(message);
    
    if (typeof Swal === 'undefined') {
        console.error('SweetAlert2 is not available! Cannot show info notification');
        alert('Thông báo: ' + message);
        return;
    }
    
    Swal.fire({
        title: 'Thông báo',
        text: message,
        icon: 'info',
        confirmButtonText: 'Đồng ý'
    });
}

// Function to confirm delete with custom message
function confirmDelete(title, text, confirmButtonText = 'Xác nhận xóa', cancelButtonText = 'Hủy') {
    console.log('confirmDelete called with:', { title, text });
    title = decodeHtml(title);
    text = decodeHtml(text);
    
    if (typeof Swal === 'undefined') {
        console.error('SweetAlert2 is not available! Cannot show confirmation dialog');
        const confirmed = confirm(title + '\n\n' + text);
        return Promise.resolve({ isConfirmed: confirmed });
    }
    
    return Swal.fire({
        title: title,
        text: text,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#dc3545',
        cancelButtonColor: '#6c757d',
        confirmButtonText: confirmButtonText,
        cancelButtonText: cancelButtonText,
        reverseButtons: true
    });
}

// Function to confirm action with custom message
function confirmAction(title, text, confirmButtonText = 'Xác nhận', cancelButtonText = 'Hủy') {
    return Swal.fire({
        title: title,
        text: text,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#0d6efd',
        cancelButtonColor: '#6c757d',
        confirmButtonText: confirmButtonText,
        cancelButtonText: cancelButtonText,
        reverseButtons: true
    });
}

// Toast notification - compact notification that appears at the corner
function showToast(message, icon = 'success', position = 'bottom-end', timer = 3000) {
    const Toast = Swal.mixin({
        toast: true,
        position: position,
        showConfirmButton: false,
        timer: timer,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer)
            toast.addEventListener('mouseleave', Swal.resumeTimer)
        }
    });

    Toast.fire({
        icon: icon,
        title: message
    });
} 