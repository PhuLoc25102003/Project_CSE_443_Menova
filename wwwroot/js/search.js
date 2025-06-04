document.addEventListener('DOMContentLoaded', function() {
    // Elements
    const searchInput = document.querySelector('#search-input');
    const searchClearBtn = document.querySelector('.search-clear-btn');
    const searchSubmitBtn = document.querySelector('.search-submit-btn');
    const searchOverlay = document.querySelector('.search-overlay');
    const searchClose = document.querySelector('.search-close');
    const tabButtons = document.querySelectorAll('.tab-button');
    const tabContents = document.querySelectorAll('.tab-content');
    const productResults = document.getElementById('product-results');
    const collectionResults = document.getElementById('collection-results');
    const loadingIndicator = document.querySelector('.search-results-loading');
    const searchIcons = document.querySelectorAll('.search-icon');
    
    // Variables
    let searchTimeout;
    let currentSearchTerm = searchInput ? searchInput.value.trim() : '';
    
    // Check for temp data flags
    const shouldOpenSearch = document.body.getAttribute('data-open-search') === 'true';
    const initialQuery = document.body.getAttribute('data-search-query') || '';
    
    if (shouldOpenSearch) {
        showSearchOverlay();
        
        // Set search input value if provided
        if (initialQuery && searchInput) {
            searchInput.value = initialQuery;
            currentSearchTerm = initialQuery;
            
            // Show clear button
            if (searchClearBtn) {
                searchClearBtn.style.display = 'block';
            }
            
            // Trigger search
            searchProducts(initialQuery);
            searchCollections(initialQuery);
        }
    }
    
    // Functions
    function showSearchOverlay() {
        if (searchOverlay) {
            searchOverlay.classList.remove('hidden');
            document.body.style.overflow = 'hidden';
            
            // Focus search input
            if (searchInput) {
                setTimeout(() => {
                    searchInput.focus();
                }, 100);
            }
        }
    }
    
    function hideSearchOverlay() {
        if (searchOverlay) {
            searchOverlay.classList.add('hidden');
            document.body.style.overflow = '';
        }
    }
    
    function clearSearch() {
        if (searchInput) {
            searchInput.value = '';
            searchInput.focus();
            
            // Hide clear button when input is empty
            searchClearBtn.style.display = 'none';
            
            // Clear results
            if (productResults) {
                productResults.innerHTML = '<div class="search-empty-state"><div class="search-empty-icon"><i class="fas fa-search"></i></div><p>Nhập từ khóa để tìm kiếm sản phẩm</p></div>';
            }
            
            if (collectionResults) {
                collectionResults.innerHTML = '';
            }
            
            currentSearchTerm = '';
        }
    }
    
    function switchTab(tabId) {
        // Remove active class from all tabs and contents
        tabButtons.forEach(button => button.parentElement.classList.remove('active'));
        tabContents.forEach(content => content.classList.remove('active'));
        
        // Add active class to the clicked tab and its content
        const clickedButton = document.querySelector(`[data-tab="${tabId}"]`);
        if (clickedButton) {
            clickedButton.parentElement.classList.add('active');
            document.getElementById(tabId).classList.add('active');
        }
    }
    
    function searchProducts(query) {
        if (!query || query.trim() === '') {
            if (productResults) {
                productResults.innerHTML = '<div class="search-empty-state"><div class="search-empty-icon"><i class="fas fa-search"></i></div><p>Nhập từ khóa để tìm kiếm sản phẩm</p></div>';
            }
            return;
        }
        
        // Show loading indicator
        if (loadingIndicator) {
            loadingIndicator.style.display = 'flex';
        }
        
        // Perform AJAX search
        fetch(`/api/search/products?q=${encodeURIComponent(query)}`)
            .then(response => response.json())
            .then(data => {
                if (loadingIndicator) {
                    loadingIndicator.style.display = 'none';
                }
                
                if (productResults) {
                    if (data && data.length > 0) {
                        // Format products into HTML
                        let html = '<div class="search-products-grid">';
                        data.forEach(product => {
                            html += `
                                <div class="search-product-item">
                                    <a href="/Product/Detail/${product.productId}" class="search-product-link">
                                        <div class="search-product-image">
                                            <img src="${product.mainImage || '/img/product-placeholder.jpg'}" alt="${product.name}" loading="lazy">
                                        </div>
                                        <div class="search-product-info">
                                            <h3 class="search-product-name">${product.name}</h3>
                                            <div class="search-product-price">
                                                ${product.discountPrice && product.discountPrice < product.price ? 
                                                    `<span class="price-sale">${product.discountPrice.toLocaleString('vi-VN')}₫</span>
                                                    <span class="price-regular">${product.price.toLocaleString('vi-VN')}₫</span>` : 
                                                    `<span class="price-regular">${product.price.toLocaleString('vi-VN')}₫</span>`}
                                            </div>
                                        </div>
                                    </a>
                                </div>`;
                        });
                        html += '</div>';
                        productResults.innerHTML = html;
                        
                        // Add click event to search product links
                        addProductLinkHandlers();
                    } else {
                        productResults.innerHTML = '<div class="search-no-results"><p>Không tìm thấy sản phẩm phù hợp</p></div>';
                    }
                }
            })
            .catch(error => {
                console.error('Search error:', error);
                if (loadingIndicator) {
                    loadingIndicator.style.display = 'none';
                }
                if (productResults) {
                    productResults.innerHTML = '<div class="search-error"><p>Đã xảy ra lỗi khi tìm kiếm. Vui lòng thử lại sau.</p></div>';
                }
            });
    }
    
    function searchCollections(query) {
        if (!query || query.trim() === '' || !collectionResults) return;
        
        fetch(`/api/search/collections?q=${encodeURIComponent(query)}`)
            .then(response => response.json())
            .then(data => {
                if (data && data.length > 0) {
                    let html = '<div class="search-collections">';
                    data.forEach(collection => {
                        html += `
                            <div class="search-collection-item">
                                <a href="/Product/Index?categoryId=${collection.categoryId}" class="search-collection-link">
                                    <div class="search-collection-image">
                                        <img src="${collection.image || '/img/category-placeholder.jpg'}" alt="${collection.name}" loading="lazy">
                                    </div>
                                    <div class="search-collection-info">
                                        <h3 class="search-collection-name">${collection.name}</h3>
                                        <p class="search-collection-description">${collection.description || ''}</p>
                                    </div>
                                </a>
                            </div>`;
                    });
                    html += '</div>';
                    collectionResults.innerHTML = html;
                    
                    // Add click event to collection links
                    addCollectionLinkHandlers();
                } else {
                    collectionResults.innerHTML = '<div class="search-no-results"><p>Không tìm thấy bộ sưu tập phù hợp</p></div>';
                }
            })
            .catch(error => {
                console.error('Collection search error:', error);
                collectionResults.innerHTML = '<div class="search-error"><p>Đã xảy ra lỗi khi tìm kiếm. Vui lòng thử lại sau.</p></div>';
            });
    }
    
    function addProductLinkHandlers() {
        const productLinks = document.querySelectorAll('.search-product-link');
        productLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                // Allow normal navigation but hide the search overlay
                hideSearchOverlay();
            });
        });
    }
    
    function addCollectionLinkHandlers() {
        const collectionLinks = document.querySelectorAll('.search-collection-link');
        collectionLinks.forEach(link => {
            link.addEventListener('click', function(e) {
                // Allow normal navigation but hide the search overlay
                hideSearchOverlay();
            });
        });
    }
    
    // Initialize search if there's a query
    if (currentSearchTerm && !shouldOpenSearch) {
        searchProducts(currentSearchTerm);
        searchCollections(currentSearchTerm);
        
        // Show clear button if there's text
        if (searchClearBtn) {
            searchClearBtn.style.display = 'block';
        }
    }
    
    // Event Listeners for header search icons
    if (searchIcons && searchIcons.length > 0) {
        searchIcons.forEach(icon => {
            icon.addEventListener('click', function(e) {
                e.preventDefault();
                showSearchOverlay();
            });
        });
    }
    
    // Close search overlay
    if (searchClose) {
        searchClose.addEventListener('click', hideSearchOverlay);
    }
    
    // Close search when clicking Escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape' && searchOverlay && !searchOverlay.classList.contains('hidden')) {
            hideSearchOverlay();
        }
    });
    
    // Event Listeners for search input
    if (searchInput) {
        // Handle showing/hiding clear button
        searchInput.addEventListener('input', function() {
            const hasValue = this.value.trim() !== '';
            if (searchClearBtn) {
                searchClearBtn.style.display = hasValue ? 'block' : 'none';
            }
            
            // Debounce search input (search as you type)
            clearTimeout(searchTimeout);
            const query = this.value.trim();
            
            if (query.length >= 2) {
                searchTimeout = setTimeout(() => {
                    if (query !== currentSearchTerm) {
                        currentSearchTerm = query;
                        searchProducts(query);
                        searchCollections(query);
                    }
                }, 300);
            } else if (query.length === 0) {
                clearSearch();
            }
        });
        
        // Search on enter key
        searchInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                const query = this.value.trim();
                if (query.length >= 2) {
                    currentSearchTerm = query;
                    searchProducts(query);
                    searchCollections(query);
                }
            }
        });
    }
    
    // Search submit button click
    if (searchSubmitBtn) {
        searchSubmitBtn.addEventListener('click', function() {
            if (searchInput) {
                const query = searchInput.value.trim();
                if (query.length >= 2) {
                    currentSearchTerm = query;
                    searchProducts(query);
                    searchCollections(query);
                }
            }
        });
    }
    
    // Clear button click
    if (searchClearBtn) {
        searchClearBtn.addEventListener('click', clearSearch);
    }
    
    // Tab switching
    tabButtons.forEach(button => {
        button.addEventListener('click', function() {
            const tabId = this.getAttribute('data-tab');
            switchTab(tabId);
        });
    });
    
    // Initialize search overlay as hidden (unless we need to show it from TempData)
    if (searchOverlay && !shouldOpenSearch) {
        searchOverlay.classList.add('hidden');
    }
}); 