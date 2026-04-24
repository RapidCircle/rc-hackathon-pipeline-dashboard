/**
 * Pipeline Dashboard Application
 * Handles authentication, user display, and logout for the authenticated app shell.
 * Uses authService.js for all auth operations.
 */

/**
 * Escape HTML to prevent XSS
 * @param {string} text - Text to escape
 * @returns {string} Escaped text
 */
function escapeHtml(text) {
    if (text === null || text === undefined) return '';
    const map = {
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#039;'
    };
    return String(text).replace(/[&<>"']/g, m => map[m]);
}

/**
 * Display current user name in the header
 * @param {Object} user - User information object
 */
function displayCurrentUser(user) {
    const nameEl = document.getElementById('current-user-name');
    if (nameEl && user) {
        nameEl.textContent = escapeHtml(user.fullName || user.userDetails || user.email || 'User');
    }
}

/**
 * Wire the logout button to authService
 */
function setupLogout() {
    const logoutBtn = document.getElementById('logout-button');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', async () => {
            await authService.logout();
        });
    }
}

/**
 * Initialize the dashboard application
 */
async function initDashboard() {
    // Require authentication — redirects to login if not authenticated
    const authState = await authService.requireAuth();

    if (!authState.isAuthenticated) {
        // requireAuth will redirect; stop further execution
        return;
    }

    // Display user info in header
    displayCurrentUser(authState.user);

    // Wire logout
    setupLogout();
}

// Run initialization when DOM is loaded
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initDashboard);
} else {
    initDashboard();
}
