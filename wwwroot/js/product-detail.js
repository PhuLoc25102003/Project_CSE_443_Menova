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
    const variantsData = document.getElementById('variants-data');
    const variantInfo = document.getElementById('variant-info');
    const addToCartForm = document.querySelector('.product-form');
    const addToCartBtn = document.querySelector('.add-to-cart-btn');
    const buyNowBtn = document.getElementById('buy-now-btn');
    const alertMessage = document.querySelector('.alert');
    const quantityInput = document.getElementById('quantity');
    const variants = Array.from(variantsData?.options || []);
    const sizeLabel = document.querySelector('.product-sizes .option-label');
    const colorLabel = document.querySelector('.product-colors .option-label');

    let selectedSizeId = null;
    let selectedColorId = null;
    let selectedVariant = null;
    let maxStock = 10;

    // Automatically hide alert message after 5 seconds
    if (alertMessage) {
        setTimeout(() => {
            alertMessage.style.opacity = '0';
            setTimeout(() => {
                alertMessage.style.display = 'none';
            }, 300);
        }, 5000);
    }

    // When size is selected
    if (sizeOptions.length > 0) {
        sizeOptions.forEach(option => {
            option.addEventListener('click', function () {
                sizeOptions.forEach(s => s.classList.remove('active'));
                this.classList.add('active');
                selectedSizeId = this.getAttribute('data-size-id');
                
                // Update size label
                if (sizeLabel) {
                    sizeLabel.textContent = 'Kích cỡ: ' + this.textContent;
                }
                
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
                
                // Update color label
                if (colorLabel) {
                    colorLabel.textContent = 'Màu Sắc: ' + this.getAttribute('title');
                }
                
                updateSelectedVariant();
            });
        });

        // Select first color by default
        colorOptions[0].click();
    }

    // Update the variant ID based on selected size and color
    function updateSelectedVariant() {
        // Clear any previous error messages
        clearErrorMessage();
        
        // Find matching variant based on selected size and color
        if (variants && variants.length > 0) {
            if ((sizeOptions.length > 0 && !selectedSizeId) || (colorOptions.length > 0 && !selectedColorId)) {
                variantIdInput.value = '';
                selectedVariant = null;
                updateVariantInfo();
                disableButtons(true);
                return;
            }

            selectedVariant = variants.find(variant => {
                const option = variant;
                const variantSizeId = option.getAttribute('data-size-id');
                const variantColorId = option.getAttribute('data-color-id');
                
                // Match based on what's available
                if (sizeOptions.length > 0 && colorOptions.length > 0) {
                    return variantSizeId === selectedSizeId && variantColorId === selectedColorId;
                } else if (sizeOptions.length > 0) {
                    return variantSizeId === selectedSizeId;
                } else if (colorOptions.length > 0) {
                    return variantColorId === selectedColorId;
                }
                return true; // If no size or color options
            });

            if (selectedVariant) {
                variantIdInput.value = selectedVariant.value;
                maxStock = parseInt(selectedVariant.getAttribute('data-stock')) || 10;
                
                // Update quantity max based on stock
                if (quantityInput) {
                    quantityInput.max = Math.min(10, maxStock);
                    if (parseInt(quantityInput.value) > maxStock) {
                        quantityInput.value = maxStock;
                    }
                }
                
                updateVariantInfo();
                disableButtons(false);
            } else {
                variantIdInput.value = '';
                showErrorMessage('Không tìm thấy biến thể sản phẩm với kích cỡ và màu sắc này');
                updateVariantInfo();
                disableButtons(true);
            }
        } else {
            // No variants available
            variantIdInput.value = '';
            disableButtons(false);
        }
    }

    // Update variant info display
    function updateVariantInfo() {
        if (!variantInfo) return;
        
        if (selectedVariant) {
            const stock = parseInt(selectedVariant.getAttribute('data-stock'));
            const additionalPrice = parseFloat(selectedVariant.getAttribute('data-additional-price'));
            
            if (stock <= 5) {
                variantInfo.innerHTML = `<div class="variant-stock low">Còn ${stock} sản phẩm</div>`;
            } else {
                variantInfo.innerHTML = `<div class="variant-stock">Còn ${stock} sản phẩm</div>`;
            }
            
            if (additionalPrice && additionalPrice > 0) {
                variantInfo.innerHTML += `<div class="variant-price">Phụ thu: ${additionalPrice.toLocaleString('vi-VN')}₫</div>`;
            }
        } else {
            variantInfo.innerHTML = '';
        }
    }
    
    // Enable/disable buttons
    function disableButtons(disabled) {
        if (addToCartBtn) addToCartBtn.disabled = disabled;
        if (buyNowBtn) buyNowBtn.disabled = disabled;
    }
    
    // Handle form submission with validation
    if (addToCartForm) {
        addToCartForm.addEventListener('submit', function(event) {
            // Prevent form submission if validation fails
            if (!validateForm()) {
                event.preventDefault();
                return false;
            }
            
            // Normal form submission continues
            addToCartBtn.disabled = true;
            addToCartBtn.textContent = 'Đang xử lý...';
        });
    }
    
    // Handle Buy Now button click
    if (buyNowBtn) {
        buyNowBtn.addEventListener('click', function() {
            if (!validateForm()) {
                return false;
            }
            
            // Create a hidden input for the buy now option
            const buyNowInput = document.createElement('input');
            buyNowInput.type = 'hidden';
            buyNowInput.name = 'buyNow';
            buyNowInput.value = 'true';
            
            // Add to form and submit
            addToCartForm.appendChild(buyNowInput);
            buyNowBtn.disabled = true;
            buyNowBtn.textContent = 'Đang xử lý...';
            addToCartForm.submit();
        });
    }
    
    // Validate form before submission
    function validateForm() {
        // Validate size selection if sizes are available
        if (sizeOptions.length > 0 && !selectedSizeId) {
            showErrorMessage('Vui lòng chọn kích cỡ');
            return false;
        }
        
        // Validate color selection if colors are available
        if (colorOptions.length > 0 && !selectedColorId) {
            showErrorMessage('Vui lòng chọn màu sắc');
            return false;
        }
        
        // Validate quantity
        const quantity = parseInt(document.getElementById('quantity').value);
        if (isNaN(quantity) || quantity < 1) {
            showErrorMessage('Số lượng không hợp lệ');
            return false;
        }
        
        // Validate variant selected
        if ((sizeOptions.length > 0 || colorOptions.length > 0) && !selectedVariant) {
            showErrorMessage('Vui lòng chọn đúng kích cỡ và màu sắc');
            return false;
        }
        
        // Validate stock quantity
        if (quantity > maxStock) {
            showErrorMessage('Số lượng vượt quá hàng tồn kho');
            return false;
        }
        
        return true;
    }
    
    // Show error message
    function showErrorMessage(message) {
        // Create or update error message
        let errorDiv = document.querySelector('.form-error');
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.className = 'form-error';
            errorDiv.setAttribute('role', 'alert');
            addToCartForm.prepend(errorDiv);
        }
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
    }
    
    // Clear error message
    function clearErrorMessage() {
        // Remove error message if exists
        const errorDiv = document.querySelector('.form-error');
        if (errorDiv) {
            errorDiv.style.display = 'none';
        }
    }
    
    // Initialize variant selection
    updateSelectedVariant();
});

// Product quantity adjustment
function increaseQuantity() {
    const input = document.getElementById('quantity');
    let value = parseInt(input.value);
    if (isNaN(value) || value < 1) {
        value = 1;
    }
    if (value < maxStock && value < 10) {
        input.value = value + 1;
    }
}

function decreaseQuantity() {
    const input = document.getElementById('quantity');
    let value = parseInt(input.value);
    if (isNaN(value) || value <= 1) {
        value = 2;
    }
    if (value > 1) {
        input.value = value - 1;
    }
}

// Image change function for thumbnails
function changeImage(imageUrl) {
    const mainImage = document.getElementById('main-product-image');
    if (mainImage) {
        mainImage.src = imageUrl;
        
        // Update active thumbnail
        const thumbnails = document.querySelectorAll('.thumbnail');
        thumbnails.forEach(thumbnail => {
            const thumbImg = thumbnail.querySelector('img');
            if (thumbImg && thumbImg.src === imageUrl) {
                thumbnails.forEach(t => t.classList.remove('active'));
                thumbnail.classList.add('active');
            }
        });
    }
}