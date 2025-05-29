/**
 * User Grid Initialization for Admin Area
 * This file handles AG Grid setup and initialization for user management
 */

// Cell renderers for user grid
const statusCellRenderer = function(params) {
    if (params.value === true) {
        return '<div class="status-badge-container"><span class="status-badge status-active">Đang hoạt động</span></div>';
    } else {
        return '<div class="status-badge-container"><span class="status-badge status-inactive">Đã khóa</span></div>';
    }
};

const roleCellRenderer = function(params) {
    if (params.value === "Admin") {
        return '<div class="status-badge-container"><span class="status-badge status-admin">Admin</span></div>';
    } else {
        return '<div class="status-badge-container"><span class="status-badge status-customer">Khách hàng</span></div>';
    }
};

function createActionCellRenderer(editUrl, deleteUrl) {
    return function(params) {
        const userId = params.data.userId;
        const isAdminAccount = params.data.isAdminAccount;
        
        return '<div class="action-buttons">' + 
            '<a href="' + editUrl + '/' + userId + 
            '" class="btn btn-sm btn-outline-primary" title="Sửa"><i class="fas fa-edit"></i></a>' +
            '<button type="button" onclick="confirmDeleteUser(\'' + userId + '\')" class="btn btn-sm btn-outline-danger" title="Xóa"' +
            (isAdminAccount ? ' disabled' : '') + '><i class="fas fa-trash"></i></button>' +
        '</div>';
    };
}

function initializeUserGrid(gridContainerId, fallbackTableId, noDataMessageId, rowData, urlConfig) {
    console.log("Initializing user grid...");
    
    // Initialize grid only if we have the container
    const gridDiv = document.getElementById(gridContainerId);
    if (!gridDiv) {
        console.error("Grid container not found!");
        const fallbackTable = document.getElementById(fallbackTableId);
        if (fallbackTable) {
            fallbackTable.style.display = 'block';
        }
        return;
    }
    
    // If agGrid is not available, show fallback table
    if (typeof agGrid === 'undefined') {
        console.error("AG Grid library not loaded!");
        const fallbackTable = document.getElementById(fallbackTableId);
        if (fallbackTable) {
            fallbackTable.style.display = 'block';
        }
        return;
    }
    
    console.log("AG Grid found, preparing data...");
    
    // Check if we have data
    const hasData = rowData && rowData.length > 0;
    console.log("Data loaded:", hasData ? "Yes" : "No", "rowData length:", hasData ? rowData.length : 0);
    
    if (!hasData) {
        const noDataMsg = document.getElementById(noDataMessageId);
        if (noDataMsg) {
            noDataMsg.style.display = 'block';
        }
        return;
    }

    // Create action cell renderer with URLs
    const actionCellRenderer = createActionCellRenderer(
        urlConfig.editUrl,
        urlConfig.deleteUrl
    );
        
    // Column definitions
    const columnDefs = [
        { 
            field: 'fullName', 
            headerName: 'Họ và tên', 
            minWidth: 180,
            flex: 3,
            filter: 'agTextColumnFilter',
            floatingFilter: true
        },
        { 
            field: 'email', 
            headerName: 'Email', 
            minWidth: 180,
            flex: 3,
            filter: 'agTextColumnFilter',
            floatingFilter: true
        },
        { 
            field: 'phoneNumber', 
            headerName: 'Số điện thoại', 
            minWidth: 150,
            flex: 2,
            filter: 'agTextColumnFilter',
            floatingFilter: true
        },
        { 
            field: 'role', 
            headerName: 'Vai trò', 
            width: 120,
            flex: 1,
            filter: 'agSetColumnFilter',
            floatingFilter: true,
            cellRenderer: roleCellRenderer,
            cellClass: 'cell-status text-center',
            headerClass: 'text-center'
        },
        { 
            field: 'createdAt', 
            headerName: 'Ngày tạo', 
            width: 120,
            flex: 1.5,
            filter: 'agDateColumnFilter',
            floatingFilter: true
        },
        { 
            field: 'isActive', 
            headerName: 'Trạng thái', 
            width: 140,
            flex: 1,
            filter: 'agSetColumnFilter',
            floatingFilter: true,
            cellRenderer: statusCellRenderer,
            cellClass: 'cell-status text-center',
            headerClass: 'text-center',
            filterParams: {
                values: [true, false],
                cellRenderer: params => {
                    return params.value === true ? 'Đang hoạt động' : 'Đã khóa';
                }
            }
        },
        { 
            field: 'userId', 
            headerName: 'Thao tác', 
            minWidth: 120,
            width: 120,
            maxWidth: 120,
            flex: 0,
            cellRenderer: actionCellRenderer,
            filter: false,
            floatingFilter: false,
            sortable: false,
            resizable: false,
            pinned: 'right',
            cellClass: 'action-cell',
            lockPinned: true,
            suppressSizeToFit: true
        }
    ];
    
    // Grid options
    const gridOptions = {
        columnDefs: columnDefs,
        rowData: rowData,
        defaultColDef: {
            flex: 1,
            minWidth: 80,
            sortable: true,
            resizable: true,
            filter: true,
            wrapText: true,
            autoHeight: true,
            cellClass: 'align-middle'
        },
        enableRangeSelection: true,
        animateRows: true,
        pagination: true,
        paginationPageSize: 20,
        enableCellTextSelection: true,
        rowSelection: 'multiple',
        suppressRowClickSelection: true,
        domLayout: 'normal',
        suppressScrollOnNewData: true,
        headerHeight: 48,
        rowHeight: 56
    };
    
    // Filter by active status if checkbox is checked
    const showActiveOnly = document.getElementById("showActiveOnly");
    const applyActiveFilter = function() {
        if (gridOptions.api) {
            if (showActiveOnly && showActiveOnly.checked) {
                gridOptions.api.setFilterModel({
                    isActive: { 
                        filterType: 'set', 
                        values: [true]
                    }
                });
            } else {
                gridOptions.api.setFilterModel({
                    isActive: null
                });
            }
        }
    };
    
    if (showActiveOnly) {
        showActiveOnly.addEventListener('change', applyActiveFilter);
    }
    
    // Create new grid
    new agGrid.Grid(gridDiv, gridOptions);
    
    // Apply initial active filter
    setTimeout(applyActiveFilter, 0);
    
    // Add refresh button event listener
    const refreshBtn = document.getElementById('refreshGrid');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', function() {
            window.location.reload();
        });
    }
    
    // Add export button event listener
    const exportBtn = document.getElementById('exportBtn');
    if (exportBtn) {
        exportBtn.addEventListener('click', function() {
            const params = {
                fileName: 'danh_sach_khach_hang.xlsx',
                columnKeys: ['fullName', 'email', 'phoneNumber', 'role', 'createdAt'],
                onlySelected: false,
                skipColumnHeaders: false,
                processCellCallback: function(params) {
                    if (params.column.colId === 'isActive') {
                        return params.value ? 'Đang hoạt động' : 'Đã khóa';
                    }
                    return params.value;
                }
            };
            
            gridOptions.api.exportDataAsExcel(params);
        });
    }
    
    // Make sure the grid is properly sized
    window.addEventListener('resize', function() {
        setTimeout(function() {
            if (gridOptions.api) {
                gridOptions.api.sizeColumnsToFit();
            }
        }, 100);
    });
    
    // Initially size columns to fit
    setTimeout(function() {
        if (gridOptions.api) {
            gridOptions.api.sizeColumnsToFit();
        }
    }, 100);
} 