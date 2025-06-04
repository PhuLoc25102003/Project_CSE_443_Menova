// wwwroot/js/site.js
// Common JavaScript functionality

// Dropdown functionality
document.addEventListener('DOMContentLoaded', function () {
    // Add any global site JavaScript here
    
    // Fix for font-awesome icon click issues
    fixIconClickHandling();
    
    // Initialize session timeout
    initSessionTimeout();
});

/**
 * Ensures Font Awesome icons are properly clickable
 * This helps when the icon is inside an anchor tag
 */
function fixIconClickHandling() {
    // Get all font awesome icons inside anchor tags
    const iconLinks = document.querySelectorAll('a i[class*="fa-"]');
    
    iconLinks.forEach(icon => {
        const parentLink = icon.closest('a');
        if (parentLink) {
            // Prevent click from being intercepted
            icon.addEventListener('click', function(e) {
                e.stopPropagation();
                
                // Programmatically trigger the parent link click
                if (parentLink.href) {
                    if (parentLink.target === '_blank') {
                        window.open(parentLink.href, '_blank');
                    } else {
                        window.location.href = parentLink.href;
                    }
                }
            });
        }
    });
}

/**
 * Session timeout handler
 * Automatically logs out user after 5 minutes of inactivity
 * with a 60-second warning before logout
 */
function initSessionTimeout() {
    // Configuration
    const inactivityTime = 4 * 60 * 1000; // 4 minutes in milliseconds
    const warningTime = 60 * 1000; // 60 seconds warning before logout
    
    // Session variables
    let inactivityTimeout;
    let warningTimeout;
    let countdownInterval;
    let secondsRemaining = warningTime / 1000;
    
    // UI elements
    const sessionModal = document.getElementById('sessionTimeoutModal');
    const countdownEl = document.getElementById('sessionCountdown');
    const stayActiveBtn = document.getElementById('stayActiveBtn');
    const logoutNowBtn = document.getElementById('logoutNowBtn');
    
    // Array of events to track for user activity
    const activityEvents = [
        'mousedown', 'mousemove', 'keypress',
        'scroll', 'touchstart', 'click', 'keydown'
    ];
    
    // Reset the timer on any user activity
    function resetInactivityTimer() {
        clearTimeout(inactivityTimeout);
        clearTimeout(warningTimeout);
        clearInterval(countdownInterval);
        hideWarning();
        
        // Start main inactivity detection
        inactivityTimeout = setTimeout(showWarning, inactivityTime);
    }
    
    // Show warning before logout
    function showWarning() {
        // Reset countdown
        secondsRemaining = warningTime / 1000;
        updateCountdown();
        
        // Show the modal
        if (sessionModal) {
            sessionModal.classList.add('active');
        }
        
        // Start countdown
        countdownInterval = setInterval(function() {
            secondsRemaining -= 1;
            updateCountdown();
            
            if (secondsRemaining <= 0) {
                clearInterval(countdownInterval);
                logout();
            }
        }, 1000);
    }
    
    // Update countdown display
    function updateCountdown() {
        if (countdownEl) {
            countdownEl.textContent = secondsRemaining;
        }
    }
    
    // Hide warning modal
    function hideWarning() {
        if (sessionModal) {
            sessionModal.classList.remove('active');
        }
    }
    
    // Function to handle logout
    function logout() {
        window.location.href = '/Account/Logout';
    }
    
    // Initialize: Set up listeners and start the timer
    function init() {
        // Check if user is logged in
        const isLoggedIn = document.cookie.indexOf('.AspNetCore.Identity.Application') > -1;
        
        if (!isLoggedIn) return;
        
        // Add all event listeners
        activityEvents.forEach(event => {
            document.addEventListener(event, resetInactivityTimer);
        });
        
        // Setup button listeners if they exist
        if (stayActiveBtn) {
            stayActiveBtn.addEventListener('click', resetInactivityTimer);
        }
        
        if (logoutNowBtn) {
            logoutNowBtn.addEventListener('click', logout);
        }
        
        // Start the initial timer
        resetInactivityTimer();
    }
    
    // Call init function
    init();
}







