/**
 * Category Grid Initialization for Admin Area
 * This file handles AG Grid setup and initialization for category management
 */

// Cell renderers for category grid
const nameCellRenderer = function(params) {
    const level = params.data.level;
    let icon = '<i class="fas fa-folder category-icon"></i>';
    let indentClass = '';
    
    if (level === 1) {
        icon = '<i class="fas fa-file-alt subcategory-icon"></i>';
        indentClass = 'indent-1';
    } else if (level === 2) {
        icon = '<i class="fas fa-file grandchild-icon"></i>';
        indentClass = 'indent-2';
    }
    
    return '<div class="category-name-cell ' + indentClass + '">' + icon + ' ' + params.value + '</div>';
};

const parentCellRenderer = function(params) {
    if (!params.value) return '-';
    return '<span class="category-parent-badge">' + params.value + '</span>';
};

const statusCellRenderer = function(params) {
    if (params.value) {
        return '<div class="status-badge-container"><span class="status-badge status-active">Đang hiển thị</span></div>';
    } else {
        return '<div class="status-badge-container"><span class="status-badge status-inactive">Ẩn</span></div>';
    }
};

function createActionCellRenderer(createUrl, editUrl, detailsUrl, deleteFunc) {
    return function(params) {
        const categoryId = params.data.categoryId;
        const level = params.data.level;
        const categoryName = params.data.name;
        let addChildButton = '';
        
        if (level === 0) {
            addChildButton = '<a href="' + createUrl + '?parentId=' + categoryId + 
            '" class="btn btn-sm btn-outline-success" title="Thêm danh mục con"><i class="fas fa-plus-circle"></i></a>';
        }
        
        return '<div class="action-buttons">' +
            addChildButton +
            '<a href="' + editUrl + '/' + categoryId + 
            '" class="btn btn-sm btn-outline-primary" title="Sửa"><i class="fas fa-edit"></i></a>' +
            '<a href="' + detailsUrl + '/' + categoryId + 
            '" class="btn btn-sm btn-outline-info" title="Chi tiết"><i class="fas fa-eye"></i></a>' +
            '<button onclick="' + deleteFunc + '(' + categoryId + ', \'' + categoryName + '\')" class="btn btn-sm btn-outline-danger" title="Xóa"><i class="fas fa-trash"></i></button>' +
        '</div>';
    };
}

function initializeCategoryGrid(gridContainerId, fallbackTableId, noDataMessageId, rowData, urlConfig) {
    // Removed the DOMContentLoaded wrapper - now this function can be called directly
    console.log("Initializing category grid...");
    
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
    
    // Create action cell renderer with URLs and delete function
    const actionCellRenderer = createActionCellRenderer(
        urlConfig.createUrl,
        urlConfig.editUrl,
        urlConfig.detailsUrl,
        urlConfig.deleteFunc
    );
        
    // Column definitions
    const columnDefs = [
        { 
            field: 'categoryId', 
            headerName: 'ID', 
            width: 70,
            flex: 0.5,
            filter: 'agNumberColumnFilter',
            floatingFilter: true
        },
        { 
            field: 'name', 
            headerName: 'Tên danh mục', 
            minWidth: 180,
            flex: 3,
            filter: 'agTextColumnFilter',
            floatingFilter: true,
            cellRenderer: nameCellRenderer
        },
        { 
            field: 'description', 
            headerName: 'Mô tả', 
            minWidth: 200,
            flex: 4,
            filter: 'agTextColumnFilter',
            floatingFilter: true
        },
        { 
            field: 'parentName', 
            headerName: 'Danh mục cha', 
            width: 140,
            flex: 1.5,
            filter: 'agTextColumnFilter',
            floatingFilter: true,
            cellRenderer: parentCellRenderer
        },
        { 
            field: 'isActive', 
            headerName: 'Trạng thái', 
            width: 130,
            flex: 0,
            filter: 'agSetColumnFilter',
            filterParams: {
                values: [true, false],
                cellRenderer: params => params.value ? 'Đang hiển thị' : 'Ẩn'
            },
            floatingFilter: true,
            cellRenderer: statusCellRenderer,
            cellClass: 'cell-status text-center',
            autoHeight: false,
            wrapText: false
        },
        { 
            field: 'categoryId', 
            headerName: 'Thao tác', 
            minWidth: 140,
            width: 140,
            maxWidth: 140,
            flex: 0,
            cellRenderer: actionCellRenderer,
            filter: false,
            floatingFilter: false,
            sortable: false,
            resizable: false,
            pinned: 'right',
            cellClass: 'action-cell',
            headerClass: 'text-center action-header',
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
            filter: true
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
    
    // Initialize grid
    try {
        console.log("Creating grid instance...");
        const grid = new agGrid.Grid(gridDiv, gridOptions);
        console.log("Grid created successfully");
        
        // Set default filter to only show active categories
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
            const actionColumn = gridOptions.columnApi.getColumn('categoryId');
            if (actionColumn) {
                // Set the action column to exactly 140px
                gridOptions.columnApi.setColumnWidth(actionColumn, 140);
            }
        }, 200);
        
        // Add window resize handler to make grid responsive
        window.addEventListener('resize', function() {
            if (gridOptions.api) {
                setTimeout(function() {
                    gridOptions.api.sizeColumnsToFit();
                }, 100);
            }
        });
        
        // Add refresh button functionality
        const refreshBtn = document.getElementById('refreshGrid');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', function() {
                location.reload();
            });
        }
        
        // Add expand all functionality
        const expandAllBtn = document.getElementById('expandAllBtn');
        if (expandAllBtn) {
            expandAllBtn.addEventListener('click', function() {
                gridOptions.api.forEachNode(node => {
                    node.setExpanded(true);
                });
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