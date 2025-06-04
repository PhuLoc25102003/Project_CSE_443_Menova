/**
 * Products Page JavaScript
 */

document.addEventListener('DOMContentLoaded', function() {
    // Initialize filter checkboxes
    initializeFilters();
    
    // Initialize sort dropdown
    initializeSorting();
    
    // Initialize color options
    initializeColorOptions();
    
    // Initialize in-stock filter
    initializeInStockFilter();
    
    // Initialize price filter
    initializePriceFilter();
    
    // Initialize mobile sidebar
    initializeMobileSidebar();
    
    // Initialize quick-add buttons
    initializeQuickAdd();
});

/**
 * Initializes filter functionality
 */
function initializeFilters() {
    const filterItems = document.querySelectorAll('.filter-item');
    
    filterItems.forEach(item => {
        item.addEventListener('click', function(e) {
            // Don't process clicks on the item itself if we're clicking the checkbox
            if (e.target.classList.contains('filter-checkbox')) {
                toggleFilter(e.target.parentElement);
                e.preventDefault();
                e.stopPropagation();
            }
        });
    });
}

/**
 * Toggles filter checkbox state
 * @param {HTMLElement} element - The filter item element
 */
function toggleFilter(element) {
    const checkbox = element.querySelector('.filter-checkbox');
    if (checkbox.classList.contains('checked')) {
        checkbox.classList.remove('checked');
        checkbox.innerHTML = '';
    } else {
        checkbox.classList.add('checked');
        checkbox.innerHTML = '✓';
    }
}

/**
 * Applies current filters and updates product list
 */
function applyFilters() {
    // Get all checked filters
    const checkedFilters = Array.from(document.querySelectorAll('.filter-checkbox.checked'))
        .map(checkbox => checkbox.parentElement.querySelector('span').innerText);
    
    console.log('Applying filters:', checkedFilters);
    
    // Here you would typically make an AJAX call to the server
    // to fetch filtered products, or use client-side filtering
    // For demonstration, we'll just log the selected filters
}

/**
 * Initializes the sort dropdown functionality
 */
function initializeSorting() {
    const sortSelect = document.getElementById('sort');
    
    if (sortSelect) {
        sortSelect.addEventListener('change', function() {
            applySorting(this.value);
        });
        
        // Set initial selected value based on URL parameters
        const urlParams = new URLSearchParams(window.location.search);
        const sortOrder = urlParams.get('sortOrder');
        
        if (sortOrder) {
            // Find the matching option and set it as selected
            for (const option of sortSelect.options) {
                if (option.value === sortOrder) {
                    option.selected = true;
                    break;
                }
            }
        }
    }
}

/**
 * Initializes the in-stock filter functionality
 */
function initializeInStockFilter() {
    const inStockCheckbox = document.getElementById('in-stock-filter');
    
    if (inStockCheckbox) {
        inStockCheckbox.addEventListener('change', function() {
            applyInStockFilter(this.checked);
        });
    }
}

/**
 * Applies the in-stock filter
 * @param {boolean} checked - Whether the checkbox is checked
 */
function applyInStockFilter(checked) {
    // Update URL and reload page with filter
    const url = new URL(window.location);
    url.searchParams.set('inStockOnly', checked ? 'true' : 'false');
    
    // Preserve existing filters
    preserveFilters(url);
    
    // Navigate to updated URL
    window.location.href = url.toString();
}

/**
 * Initializes the price range filter functionality
 */
function initializePriceFilter() {
    const priceFromInput = document.getElementById('price-from');
    const priceToInput = document.getElementById('price-to');
    const applyButton = document.getElementById('apply-price-filter');
    
    if (applyButton) {
        applyButton.addEventListener('click', function() {
            const priceFrom = priceFromInput.value ? parseInt(priceFromInput.value) : null;
            const priceTo = priceToInput.value ? parseInt(priceToInput.value) : null;
            
            applyPriceFilter(priceFrom, priceTo);
        });
    }
}

/**
 * Applies the price range filter
 * @param {number|null} priceFrom - Minimum price
 * @param {number|null} priceTo - Maximum price
 */
function applyPriceFilter(priceFrom, priceTo) {
    // Update URL and reload page with filter
    const url = new URL(window.location);
    
    if (priceFrom !== null) {
        url.searchParams.set('priceFrom', priceFrom);
    } else {
        url.searchParams.delete('priceFrom');
    }
    
    if (priceTo !== null) {
        url.searchParams.set('priceTo', priceTo);
    } else {
        url.searchParams.delete('priceTo');
    }
    
    // Preserve existing filters
    preserveFilters(url);
    
    // Navigate to updated URL
    window.location.href = url.toString();
}

/**
 * Preserves existing filter parameters when updating URL
 * @param {URL} url - The URL object to update
 */
function preserveFilters(url) {
    // Preserve category ID if it exists
    const urlParams = new URLSearchParams(window.location.search);
    const categoryId = urlParams.get('categoryId');
    const inStockOnly = urlParams.get('inStockOnly');
    const priceFrom = urlParams.get('priceFrom');
    const priceTo = urlParams.get('priceTo');
    
    if (categoryId) {
        url.searchParams.set('categoryId', categoryId);
    }
    
    if (inStockOnly && url.searchParams.get('inStockOnly') === null) {
        url.searchParams.set('inStockOnly', inStockOnly);
    }
    
    if (priceFrom && url.searchParams.get('priceFrom') === null) {
        url.searchParams.set('priceFrom', priceFrom);
    }
    
    if (priceTo && url.searchParams.get('priceTo') === null) {
        url.searchParams.set('priceTo', priceTo);
    }
}

/**
 * Applies sorting to the product list
 * @param {string} sortOrder - The sorting order to apply
 */
function applySorting(sortOrder) {
    // Update the URL with the new sort order
    const url = new URL(window.location);
    url.searchParams.set('sortOrder', sortOrder);
    
    // Preserve existing filters
    preserveFilters(url);
    
    window.location.href = url.toString();
}

/**
 * Initializes color option selection functionality
 */
function initializeColorOptions() {
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('color-option')) {
            const parent = e.target.parentElement;
            
            // Remove selected class from all options in this group
            parent.querySelectorAll('.color-option').forEach(option => {
                option.classList.remove('selected');
            });
            
            // Add selected class to clicked option
            e.target.classList.add('selected');
        }
    });
}

/**
 * Sets up intersection observers for animations
 */
function setupAnimations() {
    // Check if IntersectionObserver is available
    if ('IntersectionObserver' in window) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    entry.target.style.animation = 'fadeIn 0.6s ease-out';
                }
            });
        });

        document.querySelectorAll('.product-card').forEach((card) => {
            observer.observe(card);
        });
    }
}

/**
 * Initializes mobile sidebar functionality
 */
function initializeMobileSidebar() {
    // Create overlay element if it doesn't exist
    let overlay = document.querySelector('.sidebar-overlay');
    if (!overlay) {
        overlay = document.createElement('div');
        overlay.className = 'sidebar-overlay';
        document.body.appendChild(overlay);
    }
    
    // Filter toggle button for mobile
    const filterToggle = document.querySelector('.filter-button');
    if (filterToggle) {
        filterToggle.addEventListener('click', function() {
            toggleMobileSidebar(true);
        });
    }
    
    // Close with overlay
    overlay.addEventListener('click', function() {
        toggleMobileSidebar(false);
    });
}

/**
 * Toggles the mobile sidebar visibility
 * @param {boolean} show - Whether to show the sidebar
 */
function toggleMobileSidebar(show) {
    const sidebar = document.querySelector('.sidebar');
    const overlay = document.querySelector('.sidebar-overlay');
    
    if (show) {
        sidebar.classList.add('show');
        overlay.classList.add('show');
        document.body.style.overflow = 'hidden'; // Prevent scrolling
    } else {
        sidebar.classList.remove('show');
        overlay.classList.remove('show');
        document.body.style.overflow = ''; // Allow scrolling again
    }
}

/**
 * Initializes quick-add button functionality
 */
function initializeQuickAdd() {
    const quickAddButtons = document.querySelectorAll('.quick-add');
    const modal = document.getElementById('quick-add-modal');
    const closeBtn = document.querySelector('.quick-add-close');
    const overlay = document.querySelector('.quick-add-overlay');
    const addToCartBtn = document.getElementById('quick-add-to-cart');
    
    if (!modal) return;
    
    // Open modal when quick-add button is clicked
    quickAddButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            const productCard = this.closest('.product-card');
            const productId = productCard.querySelector('a').getAttribute('href').split('/').pop();
            
            // Fetch product details and populate modal
            fetchProductDetails(productId);
        });
    });
    
    // Close modal when close button is clicked
    if (closeBtn) {
        closeBtn.addEventListener('click', function() {
            closeModal();
        });
    }
    
    // Close modal when overlay is clicked
    if (overlay) {
        overlay.addEventListener('click', function() {
            closeModal();
        });
    }
    
    // Close modal when Escape key is pressed
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && modal.classList.contains('show')) {
            closeModal();
        }
    });
    
    // Handle size selection
    document.addEventListener('click', function(e) {
        if (e.target.classList.contains('size-option')) {
            const sizeOptions = document.querySelectorAll('.size-option');
            sizeOptions.forEach(option => option.classList.remove('selected'));
            e.target.classList.add('selected');
        }
    });
    
    // Handle add to cart button
    if (addToCartBtn) {
        addToCartBtn.addEventListener('click', function() {
            const productId = document.getElementById('quick-add-product-image')?.getAttribute('data-product-id');
            const colorId = document.querySelector('.color-option.selected')?.getAttribute('data-color-id');
            const sizeId = document.querySelector('.size-option.selected')?.getAttribute('data-size-id');
            
            if (!colorId || !sizeId) {
                showNotification('Vui lòng chọn màu sắc và kích thước');
                return;
            }
            
            addToCart(productId, colorId, sizeId);
            closeModal();
        });
    }
}

/**
 * Fetches product details and populates the quick-add modal
 * @param {string} productId - The ID of the product
 */
function fetchProductDetails(productId) {
    // Show loading state
    document.getElementById('quick-add-modal').classList.add('show');
    document.body.style.overflow = 'hidden';
    
    // Fetch product details using AJAX
    fetch(`/Product/QuickAdd/${productId}`)
        .then(response => {
            if (!response.ok) {
                if (response.status === 400) {
                    // Product is out of stock
                    closeModal();
                    showNotification('Sản phẩm hiện đã hết hàng');
                    throw new Error('Product is out of stock');
                }
                throw new Error('Network response was not ok');
            }
            return response.text();
        })
        .then(html => {
            // Replace the modal content with the fetched HTML
            const modal = document.getElementById('quick-add-modal');
            modal.innerHTML = document.createRange().createContextualFragment(html).querySelector('.quick-add-modal').innerHTML;
            
            // Re-initialize event listeners for the new modal content
            const closeBtn = modal.querySelector('.quick-add-close');
            const overlay = modal.querySelector('.quick-add-overlay');
            const addToCartBtn = document.getElementById('quick-add-to-cart');
            
            // Close modal when close button is clicked
            if (closeBtn) {
                closeBtn.addEventListener('click', function() {
                    closeModal();
                });
            }
            
            // Close modal when overlay is clicked
            if (overlay) {
                overlay.addEventListener('click', function() {
                    closeModal();
                });
            }
            
            // Handle add to cart button
            if (addToCartBtn) {
                addToCartBtn.addEventListener('click', function() {
                    const productId = document.getElementById('quick-add-product-image').getAttribute('data-product-id');
                    const colorId = document.querySelector('.color-option.selected')?.getAttribute('data-color-id');
                    const sizeId = document.querySelector('.size-option.selected')?.getAttribute('data-size-id');
                    
                    if (!colorId || !sizeId) {
                        showNotification('Vui lòng chọn màu sắc và kích thước');
                        return;
                    }
                    
                    addToCart(productId, colorId, sizeId);
                    closeModal();
                });
            }
            
            // Handle size option selection
            document.querySelectorAll('.size-option').forEach(option => {
                option.addEventListener('click', function() {
                    document.querySelectorAll('.size-option').forEach(opt => {
                        opt.classList.remove('selected');
                    });
                    this.classList.add('selected');
                });
            });
            
            // Handle color option selection
            document.querySelectorAll('.color-option').forEach(option => {
                option.addEventListener('click', function() {
                    document.querySelectorAll('.color-option').forEach(opt => {
                        opt.classList.remove('selected');
                    });
                    this.classList.add('selected');
                });
            });
        })
        .catch(error => {
            console.error('Error fetching product details:', error);
            if (error.message !== 'Product is out of stock') {
                closeModal();
                showNotification('Đã xảy ra lỗi khi tải thông tin sản phẩm');
            }
        });
}

/**
 * Closes the quick-add modal
 */
function closeModal() {
    const modal = document.getElementById('quick-add-modal');
    if (modal) {
        modal.classList.remove('show');
        document.body.style.overflow = ''; // Allow scrolling again
    }
}

/**
 * Adds a product to the cart with selected options
 * @param {string} productId - The ID of the product
 * @param {string} colorId - The ID of the selected color
 * @param {string} sizeId - The ID of the selected size
 */
function addToCart(productId, colorId, sizeId) {
    // Check if we have valid selections
    if (!productId || !colorId || !sizeId) {
        showNotification('Vui lòng chọn màu sắc và kích thước');
        return;
    }
    
    // This would typically be an API call
    console.log('Adding product to cart:', productId, 'Color:', colorId, 'Size:', sizeId);
    
    // Show a success message
    showNotification('Sản phẩm đã được thêm vào giỏ hàng');
}

/**
 * Shows a notification message
 * @param {string} message - The message to display
 */
function showNotification(message) {
    // Create notification element if it doesn't exist
    let notification = document.querySelector('.notification');
    if (!notification) {
        notification = document.createElement('div');
        notification.className = 'notification';
        document.body.appendChild(notification);
    }
    
    // Set message and show notification
    notification.textContent = message;
    notification.classList.add('show');
    
    // Hide notification after 3 seconds
    setTimeout(() => {
        notification.classList.remove('show');
    }, 3000);
} 