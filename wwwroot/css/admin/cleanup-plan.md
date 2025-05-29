# Kế hoạch dọn dẹp CSS

## CSS trùng lặp đã phát hiện và cần loại bỏ

### 1. Card Styling (đã di chuyển vào `card-styling.css`)
- Trùng lặp trong `product-management.css` (dòng ~25-47)
- Trùng lặp trong `category-management.css` (dòng ~13-35)
- **Giải pháp**: Xóa các định nghĩa card styling từ hai file trên

### 2. Page Header Styling (đã di chuyển vào `page-header.css`)
- Trùng lặp trong `product-management.css` (dòng ~8-28)
- Trùng lặp trong `category-management.css` (dòng ~4-11)
- **Giải pháp**: Xóa các định nghĩa page header từ hai file trên

### 3. AG Grid Styling (đã di chuyển một phần vào `grid-components.css`) 
- Trùng lặp trong `product-management.css` (dòng ~95-125)
- Trùng lặp trong `category-management.css` (dòng ~83-106)
- Trùng lặp trong `ag-grid-custom.css`
- **Giải pháp**: 
  - Kiểm tra xem đã di chuyển hết vào `grid-components.css` chưa
  - Xóa phần trùng lặp trong hai file gốc

### 4. Action Buttons (đã di chuyển vào `action-buttons.css`)
- Trùng lặp trong `product-management.css` (dòng ~126-148)
- Trùng lặp trong `category-management.css` (dòng ~532-561)
- **Giải pháp**: Xóa các định nghĩa action buttons từ hai file trên

## File CSS không còn sử dụng (hoặc trùng lặp)

1. `product-management.css` - Nhiều phần đã được di chuyển vào file mới, cần xem lại còn những phần nào cần giữ
2. `category-management.css` - Tương tự, đã di chuyển nhiều phần

## Kế hoạch dọn dẹp

1. Kiểm tra các selector trong `product-management.css` và `category-management.css` xem còn phần nào đặc thù (không trùng lặp) cần giữ lại
2. Di chuyển những phần đặc thù đó vào file riêng phù hợp (nếu cần)
3. Cập nhật tham chiếu trong file CSHTML để đảm bảo bao gồm tất cả file CSS cần thiết
4. Xóa những file CSS không còn cần thiết hoặc tối ưu kích thước file

## Chiến lược thực hiện

1. Tạo các file CSS mới (đã hoàn thành)
2. Theo dõi các file CSHTML để đảm bảo chúng tham chiếu đúng các file CSS mới
3. Kiểm tra giao diện để đảm bảo tất cả các kiểu vẫn được áp dụng đúng
4. Loại bỏ các phần trùng lặp trong file CSS gốc 