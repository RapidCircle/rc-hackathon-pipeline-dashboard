/**
 * Data Helpers — Opportunity extraction, filtering, aggregation
 * All data processing happens here. UI calls these functions.
 */

/**
 * Extract all opportunities from the API response, flattened.
 * @param {Object} reportData — WeeklyPipelineSummaryDto from API
 * @returns {Array} Flat array of opportunities with type info
 */
function flattenOpportunities(reportData) {
    if (!reportData || !reportData.typeSummaries) return [];
    
    var allOpps = [];
    reportData.typeSummaries.forEach(function(typeSummary) {
        if (!typeSummary.movementCategories) return;
        typeSummary.movementCategories.forEach(function(category) {
            if (!category.opportunities) return;
            category.opportunities.forEach(function(opp) {
                opp._opportunityType = typeSummary.opportunityType; // Tag with type
                opp._category = category.category; // Tag with category
                allOpps.push(opp);
            });
        });
    });
    return allOpps;
}

/**
 * Get list of unique owners from opportunities.
 * @param {Array} opportunities — flat array from flattenOpportunities()
 * @returns {Array} Sorted list of owner names
 */
function getUniqueOwners(opportunities) {
    var owners = new Set();
    opportunities.forEach(function(opp) {
        if (opp.ownerName) owners.add(opp.ownerName);
    });
    return Array.from(owners).sort();
}

/**
 * Filter opportunities by owner.
 * @param {Array} opportunities
 * @param {string|null} ownerFilter — owner name or null for all
 * @returns {Array}
 */
function filterByOwner(opportunities, ownerFilter) {
    if (!ownerFilter) return opportunities;
    return opportunities.filter(function(opp) {
        return opp.ownerName === ownerFilter;
    });
}

/**
 * Aggregate opportunities by category.
 * @param {Array} opportunities
 * @returns {Object} Map: category => {total, count, opportunities}
 */
function aggregateByCategory(opportunities) {
    var agg = {};
    var categories = ['New', 'Won', 'Lost', 'Increase', 'Decrease', 'Removed'];
    
    categories.forEach(function(cat) {
        agg[cat] = { category: cat, total: 0, count: 0, opportunities: [] };
    });
    
    opportunities.forEach(function(opp) {
        var cat = opp._category || 'New';
        if (agg[cat]) {
            agg[cat].total += (opp.weightedRevenueChange || 0);
            agg[cat].count += 1;
            agg[cat].opportunities.push(opp);
        }
    });
    
    return agg;
}

/**
 * Aggregate opportunities by opportunity type and category.
 * @param {Array} opportunities
 * @returns {Object} Map: type => {categories aggregation}
 */
function aggregateByTypeAndCategory(opportunities) {
    var agg = {};
    var types = ['SystemIntegrationCE', 'ManagedServices'];
    var categories = ['New', 'Won', 'Lost', 'Increase', 'Decrease', 'Removed'];
    
    types.forEach(function(type) {
        agg[type] = {};
        categories.forEach(function(cat) {
            agg[type][cat] = { total: 0, count: 0 };
        });
    });
    
    opportunities.forEach(function(opp) {
        var type = opp._opportunityType || 'SystemIntegrationCE';
        var cat = opp._category || 'New';
        if (agg[type] && agg[type][cat]) {
            agg[type][cat].total += (opp.weightedRevenueChange || 0);
            agg[type][cat].count += 1;
        }
    });
    
    return agg;
}

/**
 * Aggregate opportunities by type only.
 * @param {Array} opportunities
 * @returns {Object} Map: type => {total, count}
 */
function aggregateByType(opportunities) {
    var agg = { SystemIntegrationCE: 0, ManagedServices: 0 };
    opportunities.forEach(function(opp) {
        var type = opp._opportunityType || 'SystemIntegrationCE';
        if (type in agg) {
            agg[type] += (opp.weightedRevenueChange || 0);
        }
    });
    return agg;
}

/**
 * Compute data for movement summary cards (6 categories).
 * @param {Array} opportunities
 * @returns {Object} Map: category => {total, count, percentage}
 */
function computeMovementSummary(opportunities) {
    var categoryAgg = aggregateByCategory(opportunities);
    var grandTotal = 0;
    
    Object.keys(categoryAgg).forEach(function(cat) {
        grandTotal += Math.abs(categoryAgg[cat].total);
    });
    
    // Compute percentages and clean up
    Object.keys(categoryAgg).forEach(function(cat) {
        var pct = grandTotal > 0 ? (Math.abs(categoryAgg[cat].total) / grandTotal * 100) : 0;
        categoryAgg[cat].percentage = Math.round(pct);
    });
    
    return categoryAgg;
}

/**
 * Export opportunities to CSV.
 * @param {Array} opportunities
 * @param {string} filename — optional, defaults to 'pipeline-export.csv'
 */
function exportToCSV(opportunities, filename) {
    filename = filename || 'pipeline-export-' + new Date().toISOString().split('T')[0] + '.csv';
    
    var headers = ['Customer', 'Opportunity', 'Owner', 'Stage', 'Category', 'Type', 'Amount (€)'];
    var rows = [headers];
    
    opportunities.forEach(function(opp) {
        rows.push([
            opp.customerName || '',
            opp.opportunityTitle || '',
            opp.ownerName || '',
            opp.finalSalesStage || '',
            opp._category || '',
            opp._opportunityType || '',
            opp.weightedRevenueChange || 0
        ]);
    });
    
    var csv = rows.map(function(row) {
        return row.map(function(cell) {
            // Escape quotes and wrap in quotes if needed
            var str = String(cell).replace(/"/g, '""');
            return str.includes(',') || str.includes('"') || str.includes('\n') ? '"' + str + '"' : str;
        }).join(',');
    }).join('\n');
    
    var blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    var link = document.createElement('a');
    link.setAttribute('href', URL.createObjectURL(blob));
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

/**
 * Initialize owner filter dropdown with unique owners.
 * Call this after data loads.
 * @param {Array} opportunities
 */
function initializeOwnerFilter(opportunities) {
    var ownerSelect = document.getElementById('owner-filter-select');
    if (!ownerSelect) return;
    
    var owners = getUniqueOwners(opportunities);
    var currentValue = ownerSelect.value;
    
    // Clear existing options (keep placeholder)
    while (ownerSelect.options.length > 1) {
        ownerSelect.remove(1);
    }
    
    // Add owner options
    owners.forEach(function(owner) {
        var option = document.createElement('option');
        option.value = owner;
        option.textContent = owner;
        ownerSelect.appendChild(option);
    });
    
    // Restore previous selection if it still exists
    if (currentValue && owners.includes(currentValue)) {
        ownerSelect.value = currentValue;
    }
}

/**
 * Compute data for movement bar chart (categories across both types).
 * Returns array of {category, ceTotal, msTotal, combinedTotal}
 * @param {Array} opportunities
 * @returns {Array}
 */
function getChartDataMovementBars(opportunities) {
    var typeAgg = aggregateByTypeAndCategory(opportunities);
    var categories = ['New', 'Won', 'Lost', 'Increase', 'Decrease', 'Removed'];
    var data = [];
    
    categories.forEach(function(cat) {
        data.push({
            category: cat,
            ceTotal: typeAgg.SystemIntegrationCE[cat].total,
            msTotal: typeAgg.ManagedServices[cat].total,
            combinedTotal: typeAgg.SystemIntegrationCE[cat].total + typeAgg.ManagedServices[cat].total
        });
    });
    
    return data;
}

/**
 * Compute data for type split chart (doughnut).
 * Returns {ce: amount, ms: amount, total: amount}
 * @param {Array} opportunities
 * @returns {Object}
 */
function getChartDataTypeSplit(opportunities) {
    var agg = aggregateByType(opportunities);
    return {
        ce: agg.SystemIntegrationCE,
        ms: agg.ManagedServices,
        total: agg.SystemIntegrationCE + agg.ManagedServices
    };
}

/**
 * Compute data for waterfall chart.
 * Simple waterfall: Starting Balance + New - Lost + Increase - Decrease - Removed = Ending Balance
 * Returns array of {name, value, isRunning}
 * @param {Array} opportunities
 * @param {Object} apiData — original WeeklyPipelineSummaryDto for starting value
 * @returns {Array}
 */
function getChartDataWaterfall(opportunities, apiData) {
    var categoryAgg = aggregateByCategory(opportunities);
    var typeAgg = aggregateByType(opportunities);
    
    // Use API data for starting balance if available
    var startingValue = (apiData && apiData.totalStartingWeightedValue) || 0;
    var endingValue = (apiData && apiData.totalEndingWeightedValue) || 0;
    
    return [
        { name: 'Starting Balance', value: startingValue, isRunning: false },
        { name: 'New', value: categoryAgg.New.total, isRunning: true },
        { name: 'Won (Loss)', value: -categoryAgg.Won.total, isRunning: true },
        { name: 'Lost', value: -categoryAgg.Lost.total, isRunning: true },
        { name: 'Increase', value: categoryAgg.Increase.total, isRunning: true },
        { name: 'Decrease', value: -categoryAgg.Decrease.total, isRunning: true },
        { name: 'Removed', value: -categoryAgg.Removed.total, isRunning: true },
        { name: 'Ending Balance', value: endingValue, isRunning: false }
    ];
}

/**
 * Public export function for the export button.
 * Exports current filtered opportunities.
 */
window.exportCurrentData = function() {
    var cached = window._currentOpportunitiesCache || [];
    var ownerFilter = document.getElementById('owner-filter-select');
    var selectedOwner = ownerFilter ? ownerFilter.value : null;
    var filtered = filterByOwner(cached, selectedOwner);
    
    exportToCSV(filtered, 'pipeline-opportunities-' + new Date().toISOString().split('T')[0] + '.csv');
};
