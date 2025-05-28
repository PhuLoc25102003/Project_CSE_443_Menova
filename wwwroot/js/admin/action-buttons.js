/**
 * Action Buttons Standardization Script
 * This script ensures all action buttons across the admin area have consistent styling
 */

document.addEventListener('DOMContentLoaded', function() {
    // Remove margin classes from action buttons
    standardizeActionButtons();
    
    // Handle dynamic content by re-applying on AJAX complete
    document.addEventListener('ajaxComplete', standardizeActionButtons);
    
    // If using MutationObserver to detect DOM changes
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.addedNodes.length) {
                standardizeActionButtons();
            }
        });
    });
    
    // Start observing the document for changes
    observer.observe(document.body, { childList: true, subtree: true });
});

/**
 * Standardize all action buttons on the page
 */
function standardizeActionButtons() {
    // Find all action buttons
    const actionButtons = document.querySelectorAll('.action-buttons .btn');
    
    // Remove any margin classes
    actionButtons.forEach(function(button) {
        button.classList.remove('me-1', 'me-2', 'ms-1', 'ms-2', 'mx-1', 'mx-2');
        
        // Ensure proper dimensions
        button.style.width = '32px';
        button.style.height = '32px';
        button.style.padding = '0';
        button.style.margin = '0';
        button.style.display = 'inline-flex';
        button.style.alignItems = 'center';
        button.style.justifyContent = 'center';
        
        // Fix any icon styling
        const icon = button.querySelector('i');
        if (icon) {
            icon.style.margin = '0';
            icon.style.fontSize = '14px';
        }
    });
    
    // Fix container styling
    const containers = document.querySelectorAll('.action-buttons');
    containers.forEach(function(container) {
        container.style.display = 'flex';
        container.style.alignItems = 'center';
        container.style.justifyContent = 'center';
        container.style.gap = '8px';
        container.style.width = '100%';
    });
    
    // Fix AG Grid specific styles by page type
    // For Order Management
    const orderCells = document.querySelectorAll('.order-management-page .ag-theme-alpine .action-cell');
    orderCells.forEach(function(cell) {
        cell.style.display = 'flex';
        cell.style.alignItems = 'center';
        cell.style.justifyContent = 'center';
        cell.style.padding = '0';
        cell.style.width = '90px';
    });
    
    // For Product Management
    const productCells = document.querySelectorAll('.product-management-page .ag-theme-alpine .action-cell');
    productCells.forEach(function(cell) {
        cell.style.display = 'flex';
        cell.style.alignItems = 'center';
        cell.style.justifyContent = 'center';
        cell.style.padding = '0';
        cell.style.width = '160px';
    });
    
    // For Category Management
    const categoryCells = document.querySelectorAll('.category-management-page .ag-theme-alpine .action-cell');
    categoryCells.forEach(function(cell) {
        cell.style.display = 'flex';
        cell.style.alignItems = 'center';
        cell.style.justifyContent = 'center';
        cell.style.padding = '0';
        cell.style.width = '140px';
    });
    
    // Fix fallback table action columns by page type
    // For Order Management
    const orderActionCells = document.querySelectorAll('.order-management-page table.table td:last-child, .order-management-page table.table th:last-child');
    orderActionCells.forEach(function(cell) {
        cell.style.width = '90px';
        cell.style.minWidth = '90px';
        cell.style.maxWidth = '90px';
        cell.style.padding = '0';
    });
    
    // For Product Management
    const productActionCells = document.querySelectorAll('.product-management-page table.table td:last-child, .product-management-page table.table th:last-child');
    productActionCells.forEach(function(cell) {
        cell.style.width = '160px';
        cell.style.minWidth = '160px';
        cell.style.maxWidth = '160px';
        cell.style.padding = '0';
    });
    
    // For Category Management
    const categoryActionCells = document.querySelectorAll('.category-management-page table.table td:last-child, .category-management-page table.table th:last-child');
    categoryActionCells.forEach(function(cell) {
        cell.style.width = '140px';
        cell.style.minWidth = '140px';
        cell.style.maxWidth = '140px';
        cell.style.padding = '0';
    });
}

// Expose the function globally
window.standardizeActionButtons = standardizeActionButtons; 