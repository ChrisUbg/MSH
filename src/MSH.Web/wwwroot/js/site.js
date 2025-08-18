// MSH Web Application JavaScript
// This file contains common JavaScript functionality for the MSH application

// Global variables
window.msh = window.msh || {};

// Initialize the application
document.addEventListener('DOMContentLoaded', function() {
    console.log('MSH Web Application initialized');
    
    // Initialize any common functionality here
    initializeCommonFeatures();
});

// Common features initialization
function initializeCommonFeatures() {
    // Add any common JavaScript functionality here
    console.log('Common features initialized');
}

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = window.msh;
} 