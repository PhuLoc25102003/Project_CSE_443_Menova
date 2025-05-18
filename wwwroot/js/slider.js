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