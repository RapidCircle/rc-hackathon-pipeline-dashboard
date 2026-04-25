/**
 * Pipeline Dashboard — Week Selector, Summary View & Movement Details
 * Fetches weekly pipeline report data, renders summary cards,
 * movement category breakdowns, and expandable opportunity detail tables.
 */

/* ------------------------------------------------------------------ */
/*  Category metadata                                                  */
/* ------------------------------------------------------------------ */

var CATEGORY_META = {
    'New':      { label: 'New',      description: 'New opportunities',         badgeClass: 'bg-info-light text-info' },
    'Won':      { label: 'Won',      description: 'Closed won',               badgeClass: 'bg-success-light text-success' },
    'Lost':     { label: 'Lost',     description: 'Closed lost',              badgeClass: 'bg-error-light text-error' },
    'Increase': { label: 'Increase', description: 'Weighted value increase',  badgeClass: 'bg-success-light text-success' },
    'Decrease': { label: 'Decrease', description: 'Weighted value decrease',  badgeClass: 'bg-warning-light text-warning' },
    'Removed':  { label: 'Removed',  description: 'Removed from pipeline',    badgeClass: 'bg-warning-light text-warning' }
};

var CATEGORY_ORDER = ['New', 'Won', 'Lost', 'Increase', 'Decrease', 'Removed'];

var TYPE_CONFIG = {
    'SystemIntegrationCE': { prefix: 'ce', displayName: 'System Integration (CE)', badgeClass: 'bg-primary/10 text-primary', badgeLabel: 'CE' },
    'ManagedServices':     { prefix: 'ms', displayName: 'Managed Services',        badgeClass: 'bg-primary-light/10 text-primary-dark', badgeLabel: 'MS' }
};

/* ------------------------------------------------------------------ */
/*  Utility helpers                                                    */
/* ------------------------------------------------------------------ */

/**
 * Format a number as a currency string (€).
 * @param {number} value
 * @returns {string}
 */
function formatCurrency(value) {
    if (value == null || isNaN(value)) return '—';
    var num = Number(value);
    var formatted = Math.abs(num).toLocaleString('en-IE', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    });
    return (num < 0 ? '-€' : '€') + formatted;
}

/**
 * Apply a color class to a net-change element based on sign.
 * @param {HTMLElement} el
 * @param {number} value
 */
function applyNetChangeColor(el, value) {
    el.classList.remove('text-success', 'text-error');
    if (value > 0) {
        el.classList.add('text-success');
    } else if (value < 0) {
        el.classList.add('text-error');
    }
}

/**
 * Get the most recent Monday on or before today, formatted as YYYY-MM-DD.
 * @returns {string}
 */
function getDefaultWeekStart() {
    var today = new Date();
    var day = today.getDay(); // 0=Sun, 1=Mon, ...
    var diff = (day === 0) ? 6 : day - 1;
    var monday = new Date(today);
    monday.setDate(today.getDate() - diff);
    var yyyy = monday.getFullYear();
    var mm = String(monday.getMonth() + 1).padStart(2, '0');
    var dd = String(monday.getDate()).padStart(2, '0');
    return yyyy + '-' + mm + '-' + dd;
}

/**
 * Validate a weekStart string before calling the API.
 * Returns an error message string if invalid, or null if valid.
 * @param {string} value
 * @returns {string|null}
 */
function validateWeekStart(value) {
    if (!value || typeof value !== 'string') {
        return 'Please select a valid week start date.';
    }
    // Must match YYYY-MM-DD format
    if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) {
        return 'Date must be in YYYY-MM-DD format.';
    }
    var parts = value.split('-');
    var year = parseInt(parts[0], 10);
    var month = parseInt(parts[1], 10);
    var day = parseInt(parts[2], 10);
    // Basic range checks
    if (month < 1 || month > 12 || day < 1 || day > 31) {
        return 'Please enter a valid calendar date.';
    }
    // Parse and verify the date is real (e.g., not Feb 30)
    var parsed = new Date(year, month - 1, day);
    if (parsed.getFullYear() !== year || parsed.getMonth() !== month - 1 || parsed.getDate() !== day) {
        return 'Please enter a valid calendar date.';
    }
    // Must be a Monday
    if (parsed.getDay() !== 1) {
        return 'Week start date must be a Monday. Please select a Monday.';
    }
    // Must not be in the future
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    if (parsed > today) {
        return 'Cannot select a future week. Please choose a past or current week.';
    }
    return null;
}

/** Track the last successfully loaded weekStart for "Jump to latest week" */
var _lastSuccessfulWeek = null;

/**
 * Escape HTML to prevent XSS in dynamic content.
 * @param {string} text
 * @returns {string}
 */
function escapeHtmlContent(text) {
    if (text === null || text === undefined) return '';
    var map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' };
    return String(text).replace(/[&<>"']/g, function(m) { return map[m]; });
}

/* ------------------------------------------------------------------ */
/*  Status / loading helpers                                           */
/* ------------------------------------------------------------------ */

/**
 * Show the status message area with a given message and style.
 * @param {string} message
 * @param {'info'|'error'|'warning'} type
 */
function showStatusMessage(message, type) {
    var el = document.getElementById('status-message');
    if (!el) return;
    var bgMap = { info: 'bg-info-light border-info/40 text-info', error: 'bg-error-light border-error/40 text-error', warning: 'bg-warning-light border-warning/40 text-warning' };
    el.className = 'border rounded-lg px-4 py-3 text-sm ' + (bgMap[type] || bgMap.info);
    el.textContent = message;
    el.classList.remove('hidden');
}

/**
 * Hide the status message area.
 */
function hideStatusMessage() {
    var el = document.getElementById('status-message');
    if (el) el.classList.add('hidden');
}

/**
 * Show or hide the loading spinner overlay.
 * @param {boolean} show
 */
function setLoading(show) {
    var loader = document.getElementById('loading-indicator');
    if (loader) {
        loader.classList.toggle('hidden', !show);
    }
    // Hide charts while loading
    if (show) {
        if (window.hideCharts) {
            window.hideCharts();
        }
    }
}

/* ------------------------------------------------------------------ */
/*  Summary rendering                                                  */
/* ------------------------------------------------------------------ */

/**
 * Map API opportunityType string to element ID prefix.
 * @param {string} type
 * @returns {string|null}
 */
function typePrefix(type) {
    var cfg = TYPE_CONFIG[type];
    return cfg ? cfg.prefix : null;
}

/**
 * Set text content of an element by ID.
 * @param {string} id
 * @param {string} text
 */
function setText(id, text) {
    var el = document.getElementById(id);
    if (el) el.textContent = text;
}

/**
 * Set net change value with sign prefix and color.
 * @param {string} id
 * @param {number} value
 */
function setNetChange(id, value) {
    var el = document.getElementById(id);
    if (!el) return;
    var prefix = value > 0 ? '+' : '';
    el.textContent = prefix + formatCurrency(value);
    applyNetChangeColor(el, value);
}

/**
 * Populate summary cards from the API response DTO.
 * @param {Object} data — WeeklyPipelineSummaryDto
 */
function renderSummary(data) {
    // Hide empty state
    var emptyState = document.getElementById('empty-state');
    if (emptyState) emptyState.classList.add('hidden');

    // Total row
    setText('total-start', formatCurrency(data.totalStartingWeightedValue));
    setText('total-end', formatCurrency(data.totalEndingWeightedValue));
    setNetChange('total-net', data.totalNetChange);

    // Per-type summaries
    if (data.typeSummaries) {
        for (var i = 0; i < data.typeSummaries.length; i++) {
            var ts = data.typeSummaries[i];
            var prefix = typePrefix(ts.opportunityType);
            if (!prefix) continue;

            setText(prefix + '-start', formatCurrency(ts.startingWeightedValue));
            setText(prefix + '-end', formatCurrency(ts.endingWeightedValue));
            setNetChange(prefix + '-net', ts.netChange);
        }
    }
}

/* ------------------------------------------------------------------ */
/*  Movement breakdown rendering                                       */
/* ------------------------------------------------------------------ */

/**
 * SVG chevron icon for expand/collapse toggle.
 * @param {boolean} expanded
 * @returns {string}
 */
function chevronSvg(expanded) {
    if (expanded) {
        return '<svg class="h-4 w-4 text-gray-400 transition-transform" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M5.23 7.21a.75.75 0 011.06.02L10 11.168l3.71-3.938a.75.75 0 111.08 1.04l-4.25 4.5a.75.75 0 01-1.08 0l-4.25-4.5a.75.75 0 01.02-1.06z" clip-rule="evenodd"/></svg>';
    }
    return '<svg class="h-4 w-4 text-gray-400 transition-transform" viewBox="0 0 20 20" fill="currentColor"><path fill-rule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clip-rule="evenodd"/></svg>';
}

/**
 * Build an HTML string for the opportunity detail table.
 * @param {Array} opportunities
 * @returns {string}
 */
function buildDetailTable(opportunities) {
    if (!opportunities || opportunities.length === 0) {
        return '<p class="text-sm text-gray-400 py-3">No opportunities in this category.</p>';
    }
    var html = '<div class="overflow-x-auto mt-3">';
    html += '<table class="min-w-full text-left text-sm">';
    html += '<thead><tr class="bg-gray-50 border-b border-gray-200 text-xs font-semibold text-gray-600 uppercase tracking-wider">';
    html += '<th class="px-4 py-3">Customer</th>';
    html += '<th class="px-4 py-3">Opportunity</th>';
    html += '<th class="px-4 py-3">Owner</th>';
    html += '<th class="px-4 py-3">Final Stage</th>';
    html += '<th class="px-4 py-3 text-right">Amount</th>';
    html += '</tr></thead>';
    html += '<tbody class="divide-y divide-gray-100">';
    for (var i = 0; i < opportunities.length; i++) {
        var opp = opportunities[i];
        var amount = opp.weightedRevenueChange;
        var amountStr = formatCurrency(Math.abs(amount));
        if (amount > 0) amountStr = '+' + amountStr;
        else if (amount < 0) amountStr = '-' + amountStr;
        html += '<tr class="hover:bg-gray-50">';
        html += '<td class="px-4 py-3 text-gray-700">' + escapeHtmlContent(opp.customerName) + '</td>';
        html += '<td class="px-4 py-3 text-gray-700">' + escapeHtmlContent(opp.opportunityTitle) + '</td>';
        html += '<td class="px-4 py-3 text-gray-700">' + escapeHtmlContent(opp.ownerName) + '</td>';
        html += '<td class="px-4 py-3 text-gray-700">' + escapeHtmlContent(opp.finalSalesStage) + '</td>';
        html += '<td class="px-4 py-3 text-gray-700 text-right font-medium">' + escapeHtmlContent(amountStr) + '</td>';
        html += '</tr>';
    }
    html += '</tbody></table></div>';
    return html;
}

/**
 * Build the movement category rows for one opportunity type.
 * @param {string} typeKey — e.g. 'SystemIntegrationCE'
 * @param {Array} categories — movementCategories array from API
 * @returns {string} HTML
 */
function buildCategoryRows(typeKey, categories) {
    var prefix = typePrefix(typeKey);
    // Build a map from category name to its data
    var catMap = {};
    if (categories) {
        for (var i = 0; i < categories.length; i++) {
            catMap[categories[i].category] = categories[i];
        }
    }

    var html = '';
    for (var c = 0; c < CATEGORY_ORDER.length; c++) {
        var catName = CATEGORY_ORDER[c];
        var meta = CATEGORY_META[catName];
        var catData = catMap[catName] || { totalWeightedRevenueChange: 0, opportunityCount: 0, opportunities: [] };
        var totalId = prefix + '-' + catName.toLowerCase() + '-total';
        var detailId = prefix + '-' + catName.toLowerCase() + '-detail';
        var toggleId = prefix + '-' + catName.toLowerCase() + '-toggle';
        var isLast = (c === CATEGORY_ORDER.length - 1);
        var borderClass = isLast ? '' : ' border-b border-gray-200';
        var count = catData.opportunityCount || (catData.opportunities ? catData.opportunities.length : 0);

        html += '<div>';
        // Category header row
        html += '<div class="flex items-center justify-between cursor-pointer py-3' + borderClass + '" data-toggle="' + detailId + '" id="' + toggleId + '" role="button" tabindex="0" aria-expanded="false" aria-controls="' + detailId + '">';
        html += '<div class="flex items-center gap-2">';
        html += '<span id="' + toggleId + '-chevron">' + chevronSvg(false) + '</span>';
        html += '<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ' + meta.badgeClass + '">' + escapeHtmlContent(meta.label) + '</span>';
        html += '<span class="text-sm text-gray-700">' + escapeHtmlContent(meta.description) + '</span>';
        if (count > 0) {
            html += '<span class="inline-flex items-center justify-center h-5 min-w-[1.25rem] px-1.5 rounded-full text-xs font-medium bg-gray-100 text-gray-600">' + count + '</span>';
        }
        html += '</div>';
        html += '<span class="text-sm font-medium text-gray-900" id="' + totalId + '">' + formatCurrency(catData.totalWeightedRevenueChange) + '</span>';
        html += '</div>';
        // Collapsible detail area
        html += '<div id="' + detailId + '" class="hidden">';
        html += buildDetailTable(catData.opportunities);
        html += '</div>';
        html += '</div>';
    }
    return html;
}

/**
 * Build and render the full movement breakdown section.
 * @param {Array} typeSummaries — from API response
 */
function renderMovementBreakdown(typeSummaries) {
    var container = document.getElementById('movement-breakdown-container');
    if (!container) return;

    // Determine which types we have data for — always render both known types
    var typeKeys = ['SystemIntegrationCE', 'ManagedServices'];
    var typeMap = {};
    if (typeSummaries) {
        for (var i = 0; i < typeSummaries.length; i++) {
            typeMap[typeSummaries[i].opportunityType] = typeSummaries[i];
        }
    }

    var html = '';
    for (var t = 0; t < typeKeys.length; t++) {
        var typeKey = typeKeys[t];
        var cfg = TYPE_CONFIG[typeKey];
        var ts = typeMap[typeKey];
        var categories = ts ? ts.movementCategories : [];

        html += '<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">';
        html += '<div class="flex items-center gap-2 mb-4">';
        html += '<h3 class="text-sm font-semibold text-gray-900 font-heading">' + escapeHtmlContent(cfg.displayName) + '</h3>';
        html += '<span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ' + cfg.badgeClass + '">' + escapeHtmlContent(cfg.badgeLabel) + '</span>';
        html += '</div>';
        html += '<div class="space-y-0">';
        html += buildCategoryRows(typeKey, categories);
        html += '</div>';
        html += '</div>';
    }

    container.innerHTML = html;

    // Attach expand/collapse listeners
    var toggles = container.querySelectorAll('[data-toggle]');
    for (var j = 0; j < toggles.length; j++) {
        (function(toggle) {
            var handler = function(e) {
                // Allow keyboard activation via Enter/Space
                if (e.type === 'keydown' && e.key !== 'Enter' && e.key !== ' ') return;
                if (e.type === 'keydown') e.preventDefault();
                var targetId = toggle.getAttribute('data-toggle');
                var target = document.getElementById(targetId);
                var chevron = document.getElementById(toggle.id + '-chevron');
                if (target) {
                    var isHidden = target.classList.contains('hidden');
                    target.classList.toggle('hidden');
                    toggle.setAttribute('aria-expanded', isHidden ? 'true' : 'false');
                    if (chevron) {
                        chevron.innerHTML = chevronSvg(isHidden);
                    }
                }
            };
            toggle.addEventListener('click', handler);
            toggle.addEventListener('keydown', handler);
        })(toggles[j]);
    }
}

/* ------------------------------------------------------------------ */
/*  Reset / clear                                                      */
/* ------------------------------------------------------------------ */

/**
 * Reset all summary card values to dash placeholders and clear movement breakdown.
 */
function resetSummary() {\n    var ids = [\n        'ce-start', 'ce-end', 'ce-net',\n        'ms-start', 'ms-end', 'ms-net',\n        'total-start', 'total-end', 'total-net'\n    ];\n    for (var i = 0; i < ids.length; i++) {\n        var el = document.getElementById(ids[i]);\n        if (el) {\n            el.textContent = '—';\n            el.classList.remove('text-success', 'text-error');\n        }\n    }\n    // Clear movement breakdown\n    var container = document.getElementById('movement-breakdown-container');\n    if (container) container.innerHTML = '';\n\n    // Clear movement summary\n    var summaryContainer = document.getElementById('movement-summary-container');\n    if (summaryContainer) summaryContainer.innerHTML = '';\n\n    // Hide charts section\n    if (window.hideCharts) {\n        window.hideCharts();\n    }\n\n    var emptyState = document.getElementById('empty-state');\n    if (emptyState) emptyState.classList.remove('hidden');\n}

/* ------------------------------------------------------------------ */
/*  API fetch                                                          */
/* ------------------------------------------------------------------ */

/**
 * Fetch the weekly pipeline report for the selected week.
 * Validates input client-side before calling the API and maps
 * HTTP status codes to user-friendly messages.
 * @param {string} weekStart — YYYY-MM-DD
 */
async function fetchWeeklyReport(weekStart) {
    // Client-side validation before calling the API
    var validationError = validateWeekStart(weekStart);
    if (validationError) {
        showStatusMessage(validationError, 'warning');
        return;
    }

    hideStatusMessage();
    setLoading(true);
    resetSummary();

    try {
        var response = await authService.fetchWithAuth(
            '/api/pipeline/weekly-report?weekStart=' + encodeURIComponent(weekStart)
        );

        if (response.status === 400) {
            var body = await response.json().catch(function() { return {}; });
            showStatusMessage(
                (body.error || 'Invalid week start date.') +
                ' Please select a Monday in YYYY-MM-DD format.',
                'warning'
            );
            updateLatestWeekButton();
            return;
        }

        if (response.status === 404) {
            showStatusMessage(
                'No pipeline data found for the week of ' + weekStart +
                '. Try selecting a different week or seed demo data from the homepage.',
                'info'
            );
            updateLatestWeekButton();
            return;
        }

        if (!response.ok) {
            showStatusMessage(
                'Something went wrong while loading the report (HTTP ' + response.status +
                '). Please try again later.',
                'error'
            );
            return;
        }

        var data = await response.json();
        _lastSuccessfulWeek = weekStart;
        renderSummary(data);
        renderMovementBreakdown(data.typeSummaries);
        
        // Cache opportunities and initialize owner filter
        var allOpps = flattenOpportunities(data);
        initializeOwnerFilter(allOpps);
        window._currentOpportunitiesCache = allOpps;
        window._lastApiData = data;
        
        // Render charts with all opportunities
        if (window.renderAllCharts) {
            window.renderAllCharts(data, allOpps);
        }
        
        updateLatestWeekButton();
    } catch (err) {
        showStatusMessage('Unable to reach the server. Please check your connection and try again.', 'error');
    } finally {
        setLoading(false);
    }
}

/**
 * Show or hide the "Jump to latest week" button depending on context.
 * Visible when the current input differs from the last successful week.
 */
function updateLatestWeekButton() {
    var btn = document.getElementById('jump-latest-week');
    if (!btn) return;
    var weekInput = document.getElementById('week-start');
    var currentValue = weekInput ? weekInput.value : '';
    if (_lastSuccessfulWeek && currentValue !== _lastSuccessfulWeek) {
        btn.classList.remove('hidden');
    } else {
        btn.classList.add('hidden');
    }
}

/* ------------------------------------------------------------------ */
/*  Initialization                                                     */
/* ------------------------------------------------------------------ */

/**
 * Initialize the week selector and event listeners.
 */
function initWeekSelector() {
    var weekInput = document.getElementById('week-start');
    var applyBtn = document.getElementById('apply-week');
    var latestBtn = document.getElementById('jump-latest-week');

    if (weekInput) {
        weekInput.value = getDefaultWeekStart();
    }

    if (applyBtn) {
        applyBtn.addEventListener('click', function() {
            var weekStart = weekInput ? weekInput.value : '';
            fetchWeeklyReport(weekStart);
        });
    }

    // Also trigger on Enter key in the date input
    if (weekInput) {
        weekInput.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                fetchWeeklyReport(weekInput.value);
            }
        });
    }

    // "Jump to latest week" button — loads the last successfully fetched week
    if (latestBtn) {
        latestBtn.addEventListener('click', function() {
            if (_lastSuccessfulWeek && weekInput) {
                weekInput.value = _lastSuccessfulWeek;
                fetchWeeklyReport(_lastSuccessfulWeek);
            }
        });
    }

    // Owner filter dropdown event listener
    var ownerSelect = document.getElementById('owner-filter-select');
    if (ownerSelect) {
        ownerSelect.addEventListener('change', function() {
            var selectedOwner = this.value || null;
            var allOpps = window._currentOpportunitiesCache || [];
            var filtered = filterByOwner(allOpps, selectedOwner);
            
            // Re-render charts with filtered opportunities
            if (window.renderAllCharts && window._lastApiData) {
                window.renderAllCharts(window._lastApiData, filtered);
            }
        });
    }

    // Auto-load data for the default week
    if (weekInput && weekInput.value) {
        fetchWeeklyReport(weekInput.value);
    }
}

// Initialize when DOM is ready (after app.js has run auth checks)
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initWeekSelector);
} else {
    initWeekSelector();
}
