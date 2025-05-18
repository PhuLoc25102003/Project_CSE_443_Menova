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

    const productsContainer = document.querySelector('.products-container');
    const productPrevBtn = document.querySelector('.product-navigation .prev');
    const productNextBtn = document.querySelector('.product-navigation .next');

    if (productsContainer && productPrevBtn && productNextBtn) {
        productPrevBtn.addEventListener('click', () => {
            productsContainer.scrollBy({ left: -240, behavior: 'smooth' });
        });

        productNextBtn.addEventListener('click', () => {
            productsContainer.scrollBy({ left: 240, behavior: 'smooth' });
        });
    }
});

// Product sorting functionality
function updateSorting(sortValue) {
    const currentUrl = new URL(window.location.href);
    currentUrl.searchParams.set('sortOrder', sortValue);
    window.location.href = currentUrl.toString();
}