// wwwroot/js/site.js
// Common JavaScript functionality

// Dropdown functionality
document.addEventListener('DOMContentLoaded', function () {
    // Add any global site JavaScript here
    console.log('Site JavaScript loaded');
});

// wwwroot/js/slider.js
// Hero slider functionality
document.addEventListener('DOMContentLoaded', function () {
    let slideIndex = 0;
    const slides = document.querySelectorAll('.slide');
    const dots = document.querySelectorAll('.dot');

    function showSlides() {
        for (let i = 0; i < slides.length; i++) {
            slides[i].classList.remove('active');
            dots[i].classList.remove('active');
        }

        slideIndex++;
        if (slideIndex > slides.length - 1) {
            slideIndex = 0;
        }

        slides[slideIndex].classList.add('active');
        dots[slideIndex].classList.add('active');

        setTimeout(showSlides, 5000); // Change slide every 5 seconds
    }

    // Initialize slider
    if (slides.length > 0) {
        setTimeout(showSlides, 5000);
    }
});

// For manual navigation
function currentSlide(n) {
    slideIndex = n;
    const slides = document.querySelectorAll('.slide');
    const dots = document.querySelectorAll('.dot');

    for (let i = 0; i < slides.length; i++) {
        slides[i].classList.remove('active');
        dots[i].classList.remove('active');
    }

    slides[n].classList.add('active');
    dots[n].classList.add('active');
}

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

// wwwroot/js/product-detail.js
// Product detail page functionality
document.addEventListener('DOMContentLoaded', function () {
    // Image gallery functionality
    const thumbnails = document.querySelectorAll('.thumbnail');
    if (thumbnails.length > 0) {
        thumbnails.forEach(thumbnail => {
            thumbnail.addEventListener('click', function () {
                const mainImage = document.getElementById('main-product-image');
                if (mainImage) {
                    const imageUrl = this.querySelector('img').src;
                    mainImage.src = imageUrl;

                    // Update active state
                    thumbnails.forEach(t => t.classList.remove('active'));
                    this.classList.add('active');
                }
            });
        });
    }

    // Size and color selection
    const sizeOptions = document.querySelectorAll('.size-option');
    const colorOptions = document.querySelectorAll('.color-option');
    const variantIdInput = document.getElementById('selected-variant-id');

    let selectedSizeId = null;
    let selectedColorId = null;

    // Map to store variant information
    const variants = [];

    // Get variant data from data attributes or API
    if (variantIdInput) {
        // In a real implementation, this data would come from model
        // For demo, we'll assume variants are loaded from the server

        // When size is selected
        if (sizeOptions.length > 0) {
            sizeOptions.forEach(option => {
                option.addEventListener('click', function () {
                    sizeOptions.forEach(s => s.classList.remove('active'));
                    this.classList.add('active');
                    selectedSizeId = this.getAttribute('data-size-id');
                    updateSelectedVariant();
                });
            });

            // Select first size by default
            sizeOptions[0].click();
        }

        // When color is selected
        if (colorOptions.length > 0) {
            colorOptions.forEach(option => {
                option.addEventListener('click', function () {
                    colorOptions.forEach(c => c.classList.remove('active'));
                    this.classList.add('active');
                    selectedColorId = this.getAttribute('data-color-id');
                    updateSelectedVariant();
                });
            });

            // Select first color by default
            colorOptions[0].click();
        }
    }

    function updateSelectedVariant() {
        if (selectedSizeId && selectedColorId) {
            // In a real app, find the correct variant ID based on selected size and color
            // For demo purposes, we'll set a placeholder value
            variantIdInput.value = 1; // Would be replaced with actual variant ID lookup
        }
    }
});

// Product quantity adjustment
function increaseQuantity() {
    const quantityInput = document.getElementById('quantity');
    if (quantityInput) {
        let value = parseInt(quantityInput.value);
        if (value < 10) {
            quantityInput.value = value + 1;
        }
    }
}

function decreaseQuantity() {
    const quantityInput = document.getElementById('quantity');
    if (quantityInput) {
        let value = parseInt(quantityInput.value);
        if (value > 1) {
            quantityInput.value = value - 1;
        }
    }
}

// Image change function for thumbnails
function changeImage(imageUrl) {
    const mainImage = document.getElementById('main-product-image');
    if (mainImage) {
        mainImage.src = imageUrl;

        // Update active state
        const thumbnails = document.querySelectorAll('.thumbnail');
        thumbnails.forEach(thumbnail => {
            const thumbnailImg = thumbnail.querySelector('img');
            if (thumbnailImg && thumbnailImg.src === imageUrl) {
                thumbnail.classList.add('active');
            } else {
                thumbnail.classList.remove('active');
            }
        });
    }
}

// wwwroot/js/cart.js
// Cart functionality
document.addEventListener('DOMContentLoaded', function () {
    // Cart quantity management
});

// Quantity adjustment for cart items
function increaseQuantity(button) {
    const input = button.previousElementSibling;
    let value = parseInt(input.value);
    if (value < 10) {
        input.value = value + 1;
        input.form.submit();
    }
}

function decreaseQuantity(button) {
    const input = button.nextElementSibling;
    let value = parseInt(input.value);
    if (value > 1) {
        input.value = value - 1;
        input.form.submit();
    }
}