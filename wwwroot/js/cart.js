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