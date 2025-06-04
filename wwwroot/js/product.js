// wwwroot/js/product.js
// Product listing and category navigation functionality
document.addEventListener('DOMContentLoaded', function () {
    // Category and product navigation
    const categoryContainer = document.querySelector('.categories-container');
    const categoryPrevBtn = document.querySelector('.category-navigation .prev');
    const categoryNextBtn = document.querySelector('.category-navigation .next');

    if (categoryContainer && categoryPrevBtn && categoryNextBtn) {
        categoryPrevBtn.addEventListener('click', () => {
            categoryContainer.scrollBy({ left: -200, behavior: 'smooth' });
        });

        categoryNextBtn.addEventListener('click', () => {
            categoryContainer.scrollBy({ left: 200, behavior: 'smooth' });
        });
    }

    // New products navigation
    const productsContainers = document.querySelectorAll('.products-container');
    
    // Handle navigation for the first products container (new products)
    const newProductsContainer = document.querySelector('.new-products .products-container');
    const newProductPrevBtn = document.querySelector('.new-products .product-navigation .prev');
    const newProductNextBtn = document.querySelector('.new-products .product-navigation .next');

    if (newProductsContainer && newProductPrevBtn && newProductNextBtn) {
        newProductPrevBtn.addEventListener('click', () => {
            newProductsContainer.scrollBy({ left: -240, behavior: 'smooth' });
        });

        newProductNextBtn.addEventListener('click', () => {
            newProductsContainer.scrollBy({ left: 240, behavior: 'smooth' });
        });
    }
    
    // Handle navigation for the best-selling products container
    const bestSellingContainer = document.querySelector('.best-selling-products .products-container');
    const bestSellingPrevBtn = document.querySelector('.best-selling-products .product-navigation .prev');
    const bestSellingNextBtn = document.querySelector('.best-selling-products .product-navigation .next');
    
    if (bestSellingContainer && bestSellingPrevBtn && bestSellingNextBtn) {
        bestSellingPrevBtn.addEventListener('click', () => {
            bestSellingContainer.scrollBy({ left: -240, behavior: 'smooth' });
        });
        
        bestSellingNextBtn.addEventListener('click', () => {
            bestSellingContainer.scrollBy({ left: 240, behavior: 'smooth' });
        });
    }
});

// Product sorting functionality
function updateSorting(sortValue) {
    const currentUrl = new URL(window.location.href);
    currentUrl.searchParams.set('sortOrder', sortValue);
    window.location.href = currentUrl.toString();
}