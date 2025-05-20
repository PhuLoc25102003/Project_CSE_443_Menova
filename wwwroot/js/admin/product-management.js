document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips for action buttons
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    if (tooltipTriggerList.length > 0 && typeof bootstrap !== 'undefined') {
        const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl, {
            trigger: 'hover'
        }));
    }

    // Initialize DataTables
    const productTable = $('#productsTable').DataTable({
        // Responsive features
        responsive: true,
        scrollX: true,
        
        // Column resize functionality
        colResize: {
            handleWidth: 10,
            excludeColumns: [0, 8] // Exclude image and actions columns from resizing
        },
        
        // Additional features
        colReorder: true,
        fixedHeader: true,
        select: true,
        
        // Appearance options
        lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "Tất cả"]],
        pageLength: 25,
        
        // Ordering
        order: [[1, 'asc']], // Default sort by product name
        columnDefs: [
            { targets: 0, orderable: false }, // Image column
            { targets: 8, orderable: false }  // Actions column
        ],
        
        // Buttons
        dom: '<"dt-header"<"dt-header-buttons"B><"dt-header-search"f>>rt<"dt-footer"<"dt-footer-info"i><"dt-footer-pagination"p><"dt-footer-length"l>>',
        buttons: [
            {
                extend: 'collection',
                text: '<i class="fas fa-download me-1"></i> Xuất',
                className: 'btn btn-sm btn-primary',
                autoClose: true,
                buttons: [
                    {
                        extend: 'excel',
                        text: '<i class="fas fa-file-excel me-1"></i> Excel',
                        className: 'dropdown-item',
                        exportOptions: {
                            columns: ':visible:not(:last-child)'
                        }
                    },
                    {
                        extend: 'csv',
                        text: '<i class="fas fa-file-csv me-1"></i> CSV',
                        className: 'dropdown-item',
                        exportOptions: {
                            columns: ':visible:not(:last-child)'
                        }
                    },
                    {
                        extend: 'print',
                        text: '<i class="fas fa-print me-1"></i> In',
                        className: 'dropdown-item',
                        exportOptions: {
                            columns: ':visible:not(:last-child)'
                        }
                    }
                ]
            },
            {
                extend: 'colvis',
                text: '<i class="fas fa-columns me-1"></i> Cột',
                className: 'btn btn-sm btn-outline-secondary',
                columns: ':not(.noVis)'
            },
            {
                text: '<i class="fas fa-sync-alt me-1"></i> Làm mới',
                className: 'btn btn-sm btn-outline-info',
                action: function (e, dt, node, config) {
                    dt.ajax.reload();
                }
            }
        ],
        
        // Translations
        language: {
            search: "_INPUT_",
            searchPlaceholder: "Tìm kiếm sản phẩm...",
            lengthMenu: "Hiển thị _MENU_ mục",
            info: "Hiển thị _START_ đến _END_ của _TOTAL_ mục",
            infoEmpty: "Hiển thị 0 đến 0 của 0 mục",
            infoFiltered: "(lọc từ _MAX_ mục)",
            zeroRecords: "Không tìm thấy kết quả phù hợp",
            emptyTable: "Không có dữ liệu",
            paginate: {
                first: "Đầu",
                previous: "<i class='fas fa-chevron-left'></i>",
                next: "<i class='fas fa-chevron-right'></i>",
                last: "Cuối"
            },
            select: {
                rows: {
                    _: "Đã chọn %d dòng",
                    0: "Nhấp vào một dòng để chọn",
                    1: "Đã chọn 1 dòng"
                }
            },
            buttons: {
                colvis: "Hiển thị cột",
                collection: "Bộ sưu tập"
            }
        }
    });

    // Handle window resize for better responsive behavior
    window.addEventListener('resize', function() {
        productTable.columns.adjust().responsive.recalc();
    });
    
    // Make image column fixed width
    productTable.column(0).nodes().to$().css('width', '60px');
    productTable.column(8).nodes().to$().css('width', '140px');
}); 