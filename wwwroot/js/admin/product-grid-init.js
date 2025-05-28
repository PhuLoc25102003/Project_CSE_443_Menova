/**
 * Product Grid Initialization for Admin Area
 * This file handles AG Grid setup and initialization for product management
 */

// Cell renderers for product grid
const imageCellRenderer = function(params) {
    return '<img class="product-image-cell" src="' + params.value + '" alt="Product Image" />';
};

const statusCellRenderer = function(params) {
    if (params.value === true) {
        return '<div class="status-badge-container"><span class="status-badge status-active">Đang bán</span></div>';
    } else {
        return '<div class="status-badge-container"><span class="status-badge status-inactive">Ngừng bán</span></div>';
    }
};

const stockCellRenderer = function(params) {
    const value = params.value;
    
    if (value <= 0) {
        return '<div class="status-badge-container"><span class="status-badge status-outofstock">Hết hàng</span></div>';
    }
    
    if (params.data.stockDanger) {
        return '<div class="status-badge-container"><span class="status-badge status-danger">' + value + '</span></div>';
    } else if (params.data.stockWarning) {
        return '<div class="status-badge-container"><span class="status-badge status-warning">' + value + '</span></div>';
    }
    
    return '<div class="status-badge-container"><span class="status-badge status-success">' + value + '</span></div>';
};

const categoryCellRenderer = function(params) {
    if (!params.value || params.value === "Không có danh mục") {
        return '<span class="text-muted">Không có danh mục</span>';
    }
    return '<span class="category-parent-badge">' + params.value + '</span>';
};

function createActionCellRenderer(editUrl, detailsUrl, variantsUrl, deleteUrl) {
    return function(params) {
        const productId = params.data.productId;
        
        return '<div class="action-buttons">' + 
            '<a href="' + editUrl + '/' + productId + 
            '" class="btn btn-sm btn-outline-primary" title="Sửa"><i class="fas fa-edit"></i></a>' +
            '<a href="' + detailsUrl + '/' + productId + 
            '" class="btn btn-sm btn-outline-info" title="Chi tiết"><i class="fas fa-eye"></i></a>' +
            '<a href="' + variantsUrl + '/' + productId + 
            '" class="btn btn-sm btn-outline-secondary" title="Biến thể"><i class="fas fa-cubes"></i></a>' +
            '<button type="button" onclick="confirmDeleteProduct(' + productId + ')" class="btn btn-sm btn-outline-danger" title="Xóa"><i class="fas fa-trash"></i></button>' +
        '</div>';
    };
}

function initializeProductGrid(gridContainerId, fallbackTableId, noDataMessageId, rowData, urlConfig) {
    // Removed the DOMContentLoaded wrapper - now this function can be called directly
    console.log("Initializing product grid...");
    
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
        urlConfig.detailsUrl,
        urlConfig.variantsUrl,
        urlConfig.deleteUrl
    );
        
    // Column definitions
    const columnDefs = [
        { 
            field: 'imageUrl', 
            headerName: 'Ảnh', 
            width: 80,
            flex: 0.5,
            cellRenderer: imageCellRenderer,
            filter: false,
            floatingFilter: false,
            sortable: false,
            cellClass: 'cell-image'
        },
        { 
            field: 'name', 
            headerName: 'Tên sản phẩm', 
            minWidth: 180,
            flex: 3,
            filter: 'agTextColumnFilter',
            floatingFilter: true
        },
        { 
            field: 'sku', 
            headerName: 'SKU', 
            width: 120,
            flex: 1,
            filter: 'agTextColumnFilter',
            floatingFilter: true
        },
        { 
            field: 'category', 
            headerName: 'Danh mục', 
            width: 150,
            flex: 1.5,
            filter: 'agTextColumnFilter',
            floatingFilter: true,
            cellRenderer: categoryCellRenderer
        },
        { 
            field: 'formattedPrice', 
            headerName: 'Giá', 
            width: 120,
            flex: 1,
            filter: 'agNumberColumnFilter',
            floatingFilter: true,
            cellClass: 'text-end'
        },
        { 
            field: 'formattedDiscountPrice', 
            headerName: 'Giảm giá', 
            width: 120,
            flex: 1,
            filter: 'agNumberColumnFilter',
            floatingFilter: true,
            cellClass: 'text-end'
        },
        { 
            field: 'stockQuantity', 
            headerName: 'Kho hàng', 
            width: 120,
            flex: 0.8,
            filter: 'agNumberColumnFilter',
            filterParams: {
                buttons: ['apply', 'reset'],
                closeOnApply: true
            },
            floatingFilter: true,
            cellRenderer: stockCellRenderer,
            cellClass: 'text-center',
            headerClass: 'text-center'
        },
        { 
            field: 'isActive', 
            headerName: 'Trạng thái', 
            width: 130,
            flex: 0,
            filter: 'agSetColumnFilter',
            floatingFilter: true,
            cellRenderer: statusCellRenderer,
            cellClass: 'cell-status text-center',
            headerClass: 'text-center',
            autoHeight: false,
            wrapText: false,
            filterParams: {
                values: [true, false],
                cellRenderer: params => {
                    return params.value === true ? 'Đang bán' : 'Ngừng bán';
                }
            }
        },
        { 
            field: 'productId', 
            headerName: 'Thao tác', 
            minWidth: 160,
            width: 160,
            maxWidth: 160,
            flex: 0,
            cellRenderer: actionCellRenderer,
            filter: false,
            floatingFilter: false,
            sortable: false,
            resizable: false,
            pinned: 'right',
            cellClass: 'action-cell',
            lockPinned: true,
            suppressSizeToFit: true,
            cellClassRules: {
                'ag-column-thao-tac': function() { return true; }
            }
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
                        values: ['true']
                    }
                });
            } else {
                gridOptions.api.setFilterModel(null);
            }
        }
    };
    
    // Initialize grid
    try {
        console.log("Creating grid instance...");
        const grid = new agGrid.Grid(gridDiv, gridOptions);
        console.log("Grid created successfully");
        
        // Set default filter to only show active products
        gridOptions.api.setFilterModel({
            isActive: { 
                filterType: 'set', 
                values: ['true']
            }
        });
        
        // Size columns to fit after grid is ready
        setTimeout(function() {
            gridOptions.api.sizeColumnsToFit();
            
            // Make sure action column has correct width
            const actionColumn = gridOptions.columnApi.getColumn('productId');
            if (actionColumn) {
                // Set the action column to exactly 160px
                gridOptions.columnApi.setColumnWidth(actionColumn, 160);
            }
            
            // Hide any empty columns
            gridOptions.columnApi.getAllColumns().forEach(col => {
                if (!col.getColDef().field && !col.getColDef().headerName) {
                    gridOptions.columnApi.setColumnVisible(col, false);
                }
            });
        }, 200);
        
        // Add window resize handler to make grid responsive
        window.addEventListener('resize', function() {
            if (gridOptions.api) {
                setTimeout(function() {
                    gridOptions.api.sizeColumnsToFit();
                }, 100);
            }
        });
        
        // Add filter checkbox event handler
        if (showActiveOnly) {
            showActiveOnly.addEventListener('change', applyActiveFilter);
        }
        
        // Add refresh button functionality
        const refreshBtn = document.getElementById('refreshGrid');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', function() {
                location.reload();
            });
        }
        
        // Add export button functionality
        const exportBtn = document.getElementById("exportBtn");
        if (exportBtn) {
            exportBtn.addEventListener('click', function() {
                if (gridOptions.api) {
                    gridOptions.api.exportDataAsExcel({
                        fileName: 'danh_sach_san_pham_' + new Date().toISOString().slice(0, 10),
                        skipColumnGroupHeaders: true,
                        skipColumnHeaders: false,
                        skipPinnedTop: true,
                        skipPinnedBottom: true,
                        allColumns: false,
                        onlySelected: false,
                        columnKeys: ['name', 'sku', 'category', 'formattedPrice', 'formattedDiscountPrice', 'stockQuantity', 'isActive']
                    });
                }
            });
        }
        
    } catch (error) {
        console.error("Error initializing AG Grid:", error);
        const fallbackTable = document.getElementById(fallbackTableId);
        if (fallbackTable) {
            fallbackTable.style.display = 'block';
        }
        const gridContainer = document.getElementById(gridContainerId);
        if (gridContainer) {
            gridContainer.style.display = 'none';
        }
    }
} 