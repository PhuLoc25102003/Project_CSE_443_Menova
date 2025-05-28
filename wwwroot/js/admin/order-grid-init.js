/**
 * Order Grid Initialization for Admin Area
 * This file handles AG Grid setup and initialization for order management
 */

// Order status renderer function
function statusRenderer(params) {
    if (!params.value) return '';
    
    const statusMap = {
        'chờ xử lý': { text: 'Chờ xử lý', class: 'status-pending' },
        'đang xử lý': { text: 'Đang xử lý', class: 'status-processing' },
        'đang giao hàng': { text: 'Đang giao hàng', class: 'status-shipping' },
        'đã giao hàng': { text: 'Đã giao hàng', class: 'status-shipped' },
        'đã nhận hàng': { text: 'Đã nhận hàng', class: 'status-completed' },
        'đã hủy': { text: 'Đã hủy', class: 'status-cancelled' },
        'pending': { text: 'Chờ xử lý', class: 'status-pending' },
        'processing': { text: 'Đang xử lý', class: 'status-processing' },
        'shipping': { text: 'Đang giao hàng', class: 'status-shipping' },
        'delivered': { text: 'Đã giao hàng', class: 'status-shipped' },
        'received': { text: 'Đã nhận hàng', class: 'status-completed' },
        'cancelled': { text: 'Đã hủy', class: 'status-cancelled' }
    };
    
    const status = params.value.toLowerCase();
    const statusInfo = statusMap[status] || { text: params.value, class: 'status-default' };
    
    return `<div class="status-badge-container"><span class="status-badge ${statusInfo.class}">${statusInfo.text}</span></div>`;
}

// Date formatter function
function dateFormatter(params) {
    if (!params.value) return '';
    try {
        const date = new Date(params.value);
        return date.toLocaleDateString('vi-VN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    } catch (e) {
        return params.value;
    }
}

// Currency formatter function
function currencyFormatter(params) {
    if (params.value === undefined || params.value === null) return '';
    return params.value.toLocaleString('vi-VN') + ' VNĐ';
}

// Action buttons renderer function
function actionRenderer(params, urlConfig) {
    if (!params.data || !params.data.orderId) return '';
    const orderId = params.data.orderId;
    
    return `<div class="action-buttons">
        <a href="${urlConfig.detailsUrl}/${orderId}" 
           class="btn btn-sm btn-outline-info" 
           title="Xem chi tiết">
            <i class="fas fa-eye"></i>
        </a>
        <button type="button" 
                onclick="${urlConfig.deleteConfirmFn}(${orderId})" 
                class="btn btn-sm btn-outline-danger" 
                title="Xóa">
            <i class="fas fa-trash"></i>
        </button>
    </div>`;
}

function initializeOrderGrid(gridId, fallbackTableId, noDataMessageId, rowData, urlConfig, optionsOverride = {}) {
    try {
        // Check if grid element exists
        const gridDiv = document.getElementById(gridId);
        if (!gridDiv) {
            console.error("Grid container not found:", gridId);
            document.getElementById(fallbackTableId).style.display = "block";
            return null;
        }

        // Set static width based on container width
        const containerWidth = gridDiv.parentElement.clientWidth;
        // Reserve 120px for action column
        const remainingWidth = containerWidth - 120;
        // Calculate percentage for each column based on importance
        const totalFlexUnits = 100;
        const colFlexes = {
            orderId: 15,         // 15% (increased from 10%)
            orderDate: 16,       // 16%
            userName: 17,        // 17% (reduced from 20%)
            phoneNumber: 15,     // 15%
            totalAmount: 17,     // 17% (reduced from 19%)
            orderStatus: 20      // 20%
        };
        
        // Calculate actual widths
        const colWidths = {};
        for (const [colId, flex] of Object.entries(colFlexes)) {
            colWidths[colId] = Math.floor(remainingWidth * (flex / totalFlexUnits));
        }

        // Define column definitions with balanced sizing
        const columnDefs = [
            { 
                headerName: "Mã đơn hàng", 
                field: "orderId",
                width: colWidths.orderId,
                minWidth: colWidths.orderId,
                flex: colFlexes.orderId,
                cellRenderer: params => `#${params.value}`,
                filter: 'agNumberColumnFilter',
                cellClass: 'text-center',
                suppressSizeToFit: false
            },
            { 
                headerName: "Ngày đặt", 
                field: "orderDate", 
                width: colWidths.orderDate,
                minWidth: colWidths.orderDate,
                flex: colFlexes.orderDate,
                cellRenderer: dateFormatter,
                sort: 'desc',
                sortable: true,
                suppressSizeToFit: false
            },
            { 
                headerName: "Khách hàng", 
                field: "userName", 
                width: colWidths.userName,
                minWidth: colWidths.userName,
                flex: colFlexes.userName,
                hide: window.innerWidth < 768,
                suppressSizeToFit: false
            },
            { 
                headerName: "Số điện thoại", 
                field: "phoneNumber", 
                width: colWidths.phoneNumber,
                minWidth: colWidths.phoneNumber,
                flex: colFlexes.phoneNumber,
                cellClass: 'text-center',
                suppressSizeToFit: false
            },
            { 
                headerName: "Tổng tiền", 
                field: "totalAmount", 
                width: colWidths.totalAmount,
                minWidth: colWidths.totalAmount,
                flex: colFlexes.totalAmount,
                cellRenderer: currencyFormatter,
                cellClass: 'text-right',
                type: 'numericColumn',
                suppressSizeToFit: false
            },
            { 
                headerName: "Trạng thái", 
                field: "orderStatus", 
                width: colWidths.orderStatus,
                minWidth: colWidths.orderStatus,
                maxWidth: colWidths.orderStatus + 40, // Increased from +20 to +40
                flex: colFlexes.orderStatus,
                cellRenderer: statusRenderer,
                cellClass: 'text-center',
                suppressSizeToFit: false
            },
            { 
                headerName: "Thao tác", 
                field: "thaoTac",
                width: 120,
                minWidth: 120,
                maxWidth: 120,
                cellRenderer: params => actionRenderer(params, urlConfig),
                cellClass: ['action-cell', 'text-center'],
                headerClass: 'thao-tac-column-header',
                suppressMovable: true,
                pinned: 'right',
                sortable: false,
                filter: false,
                lockPinned: true
            }
        ];

        // Default grid options
        const defaultOptions = {
            columnDefs: columnDefs,
            rowData: rowData,
            defaultColDef: {
                flex: 1,
                resizable: false,
                sortable: true,
                filter: true,
                suppressSizeToFit: false
            },
            // Set fixed row height for consistency
            rowHeight: 48,
            headerHeight: 48,
            
            // Disable feature that cause performance issues
            suppressColumnVirtualisation: true,
            suppressRowVirtualisation: false,
            suppressAutoSize: true,
            suppressColumnMoveAnimation: true,
            suppressMovableColumns: true,
            suppressFieldDotNotation: true,
            
            // Disable horizontal scrolling
            suppressHorizontalScroll: true,
            
            // Performance optimizations for zoom
            enableCellChangeFlash: false,
            suppressAnimationFrame: true,
            suppressColumnStateEvents: true,
            suppressScrollOnNewData: true,
            batchUpdateWaitMillis: 150,
            
            // Regular options
            animateRows: false,
            pagination: true,
            paginationPageSize: 20,
            paginationAutoPageSize: false,
            suppressRowClickSelection: true,
            rowSelection: 'single',
            enableCellTextSelection: true,
            
            // Layout settings
            domLayout: 'normal',
            
            // Fix for empty columns
            suppressEmptyColumns: true,
            
            // Custom template
            overlayNoRowsTemplate: 
                '<div class="ag-overlay-no-rows-center">Không có dữ liệu</div>',
                
            // Grid ready event to ensure proper setup
            onGridReady: (params) => {
                // Apply immediate styling fixes
                applyFixesForGrid();
                
                // Disable zoom-related resize completely
                window.addEventListener('resize', noopDuringZoom, { passive: true });
                
                // Apply fixed column widths on init
                setTimeout(applyFixedLayout, 0);
            },
        };
        
        // Apply immediate fixes for the grid
        function applyFixesForGrid() {
            // Force remove all borders
            document.querySelectorAll('.ag-theme-alpine *').forEach(el => {
                el.style.border = 'none';
                el.style.boxShadow = 'none';
            });
            
            // Fix row heights to prevent partial rows
            document.querySelectorAll('.ag-row').forEach(row => {
                row.style.height = '48px';
                row.style.minHeight = '48px';
                row.style.maxHeight = '48px';
                row.style.borderBottom = '1px solid #f0f0f0';
                row.style.borderTop = 'none';
                row.style.borderLeft = 'none';
                row.style.borderRight = 'none';
            });
            
            // Make pinned column transparent
            document.querySelectorAll('.ag-pinned-right-header, .ag-pinned-right-cols-container, .ag-pinned-right-cols-viewport').forEach(el => {
                el.style.border = 'none';
                el.style.borderLeft = 'none';
                el.style.borderRight = 'none';
                el.style.boxShadow = 'none';
                el.style.background = 'transparent';
            });
            
            // Fix grid height to show all rows
            document.getElementById(gridId).style.height = '700px';
        }
        
        // Apply fixed column layout to prevent resizing issues
        function applyFixedLayout() {
            // Set container width to prevent expansion
            const container = document.querySelector('.ag-center-cols-container');
            if (container) {
                container.style.width = '100%';
                container.style.minWidth = '100%';
                container.style.maxWidth = '100%';
            }
            
            // Apply width to all cells and prevent borders
            document.querySelectorAll('.ag-cell, .ag-header-cell').forEach(cell => {
                cell.style.border = 'none';
                cell.style.borderRight = 'none';
                cell.style.borderLeft = 'none';
            });
        }
        
        // Completely disable resize handling during zoom
        function noopDuringZoom() {
            // Do nothing - this prevents any recalculation during zoom
        }
        
        // Clean up any previous grid
        while (gridDiv.firstChild) {
            gridDiv.removeChild(gridDiv.firstChild);
        }

        // Initialize the grid
        const grid = new agGrid.Grid(gridDiv, defaultOptions);

        // Add export functionality
        const exportBtn = document.getElementById('exportOrders');
        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                if (grid.api) {
                    grid.api.exportDataAsCsv({
                        fileName: 'orders_export_' + new Date().toISOString().slice(0, 10) + '.csv',
                        processCellCallback: function(params) {
                            // Format data for export
                            if (params.column.colId === 'orderDate') {
                                return dateFormatter({ value: params.value }).replace(/<[^>]*>/g, '');
                            }
                            if (params.column.colId === 'totalAmount') {
                                return currencyFormatter({ value: params.value }).replace(/<[^>]*>/g, '');
                            }
                            if (params.column.colId === 'orderStatus') {
                                const statusContent = statusRenderer({ value: params.value });
                                return statusContent.replace(/<[^>]*>/g, '');
                            }
                            return params.value;
                        }
                    });
                }
            });
        }
        
        // Add refresh button handler
        const refreshBtn = document.getElementById('refreshGrid');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', function() {
                location.reload();
            });
        }
        
        // Apply style fixes with delay to ensure they override AG Grid defaults
        setTimeout(applyFixesForGrid, 0);
        setTimeout(applyFixedLayout, 0);
        
        // Apply fixes after pagination
        setTimeout(applyFixesForGrid, 500);
        
        // Apply more persistent fixes to prevent overrides from AG Grid
        const styleSheet = document.createElement('style');
        styleSheet.type = 'text/css';
        styleSheet.innerText = `
            .ag-theme-alpine * { border: none !important; box-shadow: none !important; }
            .ag-theme-alpine .ag-pinned-right-header,
            .ag-theme-alpine .ag-pinned-right-cols-container,
            .ag-theme-alpine .ag-pinned-right-cols-viewport { 
                border: none !important; 
                border-left: none !important;
                background: transparent !important;
                box-shadow: none !important;
            }
            .ag-theme-alpine .ag-row {
                height: 48px !important;
                min-height: 48px !important;
                max-height: 48px !important;
                border-bottom: 1px solid #f0f0f0 !important;
                border-top: none !important;
                border-left: none !important;
                border-right: none !important;
            }
            .ag-theme-alpine .ag-cell {
                border: none !important;
                border-right: none !important;
                border-left: none !important;
            }
            #${gridId} {
                height: 700px !important;
                overflow: hidden !important;
            }
        `;
        document.head.appendChild(styleSheet);

        // Merge default options with override
        const gridOptions = { ...defaultOptions, ...optionsOverride };
        
        // Return grid options
        return gridOptions;
    } catch (error) {
        console.error("Error initializing grid:", error);
        
        // Show fallback table on error
        document.getElementById(fallbackTableId).style.display = "block";
        document.getElementById(gridId).style.display = "none";
        document.getElementById(noDataMessageId).style.display = "none";
        
        return null;
    }
} 