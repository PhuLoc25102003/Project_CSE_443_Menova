/**
 * Order Filter Visibility Fix
 * This script ensures that the order search container and input elements
 * remain visible regardless of any CSS that might be hiding them.
 */
(function() {
  // Run on DOMContentLoaded and after a slight delay to override any late-loading styles
  document.addEventListener('DOMContentLoaded', function() {
    // Immediate fix
    fixSearchVisibility();
    
    // Fix again after a delay (in case of other scripts or CSS loading after)
    setTimeout(fixSearchVisibility, 100);
    setTimeout(fixSearchVisibility, 500);
    setTimeout(fixSearchVisibility, 1000);
  });
  
  // Run when window loads (final attempt)
  window.addEventListener('load', fixSearchVisibility);
  
  function fixSearchVisibility() {
    // Elements to ensure visibility
    const elements = [
      document.getElementById('orderSearchContainer'),
      document.getElementById('searchQueryInput'),
      document.querySelector('.filter-search'),
      document.querySelector('.search-input-group')
    ];
    
    // Apply critical visibility styles to each element
    elements.forEach(el => {
      if (el) {
        // Remove any problematic classes that might be hiding the element
        ['hidden', 'invisible', 'd-none', 'collapse', 'collapsed'].forEach(className => {
          el.classList.remove(className);
        });
      }
    });
    
    // Special case for search container - must be flex!
    const searchContainer = document.getElementById('orderSearchContainer');
    if (searchContainer) {
      searchContainer.style.cssText = `
        display: flex !important;
        flex-direction: row !important;
        flex-wrap: nowrap !important;
        gap: 1rem !important;
        visibility: visible !important;
        opacity: 1 !important;
        width: 100% !important;
        max-width: 100% !important;
        align-items: center !important;
        max-height: none !important;
        overflow: visible !important;
        position: static !important;
        transform: none !important;
        pointer-events: auto !important;
        z-index: 10 !important;
      `;
      
      // Ensure parent elements are also visible
      let parent = searchContainer.parentElement;
      while (parent && parent !== document.body) {
        parent.style.cssText += `
          display: block !important;
          visibility: visible !important;
          opacity: 1 !important;
          overflow: visible !important;
          max-height: none !important;
        `;
        parent = parent.parentElement;
      }
    }
    
    // Set specific styles for search input group
    const searchInputGroup = document.querySelector('.search-input-group');
    if (searchInputGroup) {
      searchInputGroup.style.cssText = `
        display: flex !important;
        flex: 2 !important;
        min-width: 200px !important;
        max-width: 60% !important;
        visibility: visible !important;
        opacity: 1 !important;
      `;
    }
    
    // Set specific styles for date filter group
    const dateFilterGroup = document.querySelector('.date-filter-group');
    if (dateFilterGroup) {
      dateFilterGroup.style.cssText = `
        display: flex !important;
        flex: 1 !important;
        min-width: 200px !important;
        max-width: 40% !important;
        visibility: visible !important;
        opacity: 1 !important;
      `;
    }
    
    // Ensure search input is visible with properly displayed placeholder
    const searchInput = document.getElementById('searchQueryInput');
    if (searchInput) {
      searchInput.style.cssText = `
        display: block !important;
        visibility: visible !important;
        opacity: 1 !important;
        width: 100% !important;
        min-width: 150px !important;
        border: none !important;
        outline: none !important;
        padding: 0.625rem 0.5rem 0.625rem 0.25rem !important;
        font-size: 0.875rem !important;
        background: transparent !important;
        color: #333 !important;
      `;
      
      // Force placeholder to be visible
      if (!searchInput.value) {
        // Briefly remove and reapply placeholder to force browser to recognize it
        const placeholder = searchInput.getAttribute('placeholder');
        searchInput.setAttribute('placeholder', '');
        setTimeout(() => {
          searchInput.setAttribute('placeholder', placeholder);
        }, 10);
      }
    }
    
    // Ensure the filter buttons container is visible
    const filterButtons = document.querySelector('.filter-buttons');
    if (filterButtons) {
      filterButtons.style.cssText += `
        display: flex !important;
        visibility: visible !important;
        opacity: 1 !important;
      `;
    }
    
    // Handle responsive layout based on screen width
    if (window.innerWidth <= 992) {
      if (searchContainer) {
        searchContainer.style.flexWrap = 'wrap !important';
      }
      
      if (searchInputGroup) {
        searchInputGroup.style.cssText += `
          flex: 1 0 100% !important;
          max-width: 100% !important;
          margin-bottom: 0.75rem !important;
        `;
      }
      
      if (dateFilterGroup) {
        dateFilterGroup.style.cssText += `
          flex: 1 0 100% !important;
          max-width: 100% !important;
          margin-bottom: 0.75rem !important;
        `;
      }
    }
    
    console.log('Search visibility fix applied - layout optimized for horizontal alignment');
  }
})(); 