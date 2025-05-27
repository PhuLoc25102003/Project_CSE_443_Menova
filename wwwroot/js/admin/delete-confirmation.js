/**
 * Delete confirmation handler
 */
console.log('Delete confirmation script loaded');

document.addEventListener('DOMContentLoaded', function() {
    console.log('Setting up delete confirmations from specific script');
    
    // Test SweetAlert
    console.log('SweetAlert is available:', typeof Swal !== 'undefined');
    
    // Setup direct delete buttons using event delegation
    document.body.addEventListener('click', function(e) {
        // Tìm nút xóa được click
        if (e.target.closest('a[href*="Delete"]')) {
            const link = e.target.closest('a[href*="Delete"]');
            const href = link.getAttribute('href');
            
            console.log('Delete link clicked:', href);
            
            // Ngăn chặn hành động mặc định
            e.preventDefault();
            e.stopPropagation();
            
            // Xác định loại mục đang được xóa
            const itemType = href.includes('Category') ? 'danh mục' : 'sản phẩm';
            const row = link.closest('tr');
            let itemName = '';
            
            // Cố gắng lấy tên từ dòng trong bảng
            if (row) {
                const nameCell = row.querySelector('td:nth-child(2)');
                if (nameCell) {
                    itemName = nameCell.innerText.trim();
                }
            }
            
            // Hiển thị hộp thoại xác nhận
            // Decode HTML entities trong title và text
            const decodeHtml = function(html) {
                var txt = document.createElement("textarea");
                txt.innerHTML = html;
                return txt.value;
            };
            
            // Chuẩn bị tiêu đề và nội dung
            let title = `Xác nhận xóa ${itemType}`;
            let text = itemName ? 
                      `Bạn có chắc muốn xóa ${itemType} "${itemName}" không?` : 
                      `Bạn có chắc muốn xóa ${itemType} này không?`;
            
            // Decode để đảm bảo hiển thị đúng
            title = decodeHtml(title);
            text = decodeHtml(text);
            
            Swal.fire({
                title: title,
                text: text,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#dc3545',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Xác nhận xóa',
                cancelButtonText: 'Hủy',
                reverseButtons: true
            }).then((result) => {
                if (result.isConfirmed) {
                    // Tạo form để submit
                    const form = document.createElement('form');
                    form.method = 'post';
                    form.action = href;
                    form.style.display = 'none';
                    
                    // Thêm token
                    const token = document.querySelector('input[name="__RequestVerificationToken"]');
                    if (token) {
                        const tokenInput = document.createElement('input');
                        tokenInput.type = 'hidden';
                        tokenInput.name = '__RequestVerificationToken';
                        tokenInput.value = token.value;
                        form.appendChild(tokenInput);
                    }
                    
                    // Lấy ID từ URL
                    const idMatch = href.match(/\/Delete\/(\d+)/) || 
                                   href.match(/[?&]id=(\d+)/) || 
                                   href.match(/\/Delete\?id=(\d+)/);
                    
                    if (idMatch && idMatch[1]) {
                        const idInput = document.createElement('input');
                        idInput.type = 'hidden';
                        idInput.name = 'id';
                        idInput.value = idMatch[1];
                        form.appendChild(idInput);
                    }
                    
                    // Submit form
                    document.body.appendChild(form);
                    form.submit();
                }
            });
        }
    });
}); 