// wwwroot/js/cart.js
// Cart functionality
document.addEventListener('DOMContentLoaded', function () {
    // Show alert messages if they exist and fade them out
    const alertMessage = document.querySelector('.alert');
    if (alertMessage) {
        setTimeout(() => {
            alertMessage.style.opacity = '0';
            setTimeout(() => {
                alertMessage.style.display = 'none';
            }, 300);
        }, 5000);
    }
});

// Quantity adjustment for cart items
function increaseQuantity(button) {
    const input = button.previousElementSibling;
    let value = parseInt(input.value);
    if (isNaN(value) || value < 1) {
        value = 1;
    }
    if (value < 10) {
        input.value = value + 1;
        input.form.submit();
    }
}

function decreaseQuantity(button) {
    const input = button.nextElementSibling;
    let value = parseInt(input.value);
    if (isNaN(value) || value <= 1) {
        value = 2;
    }
    if (value > 1) {
        input.value = value - 1;
        input.form.submit();
    }
}