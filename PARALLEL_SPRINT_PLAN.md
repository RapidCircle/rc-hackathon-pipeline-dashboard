# 🚀 Parallel Execution Plan — 3-Hour Hackathon Sprint
**Pipeline Change Analysis Dashboard — 2-Developer Sprint**

---

## 📋 EXECUTIVE SUMMARY

**Goal**: Add charts, filters, export, and visual polish to existing dashboard.  
**Duration**: 3 hours  
**Team**: Dev A (Data + Logic), Dev B (UI + Charts)  
**Key Principle**: No file conflicts. Dev A owns data processing. Dev B owns rendering.

**Demo Priority Order**:
1. ✅ Owner Filter (functional)
2. ✅ Movement Summary Cards (6-card visual grid)
3. ✅ Movement Bar Chart (by category, per type)
4. ✅ CSV Export
5. ✅ Type Split Chart (doughnut)
6. ✅ Waterfall Chart (if time)
7. ⏭ Weekly Comparison (fallback)

---

## ⏰ TIMELINE (180 minutes)

| Phase | Duration | Dev A | Dev B | Checkpoint |
|-------|----------|-------|-------|-----------|
| **Phase 1: Setup & Boilerplate** | 0–25 min | Create `dataHelpers.js` scaffold | Create chart containers in HTML | Both files created |
| **Phase 2: Core Filtering & Data** | 25–65 min | Owner filter + data aggregation | HTML layout + Tailwind polish | Data functions ready, HTML structure set |
| **Phase 3: Charts & Export** | 65–120 min | CSV export + data helpers finalized | Chart.js integration + rendering | Charts rendering, export button wired |
| **Phase 4: Integration & Refinement** | 120–165 min | Test data flow | Test UI + visual polish | Full feature set working |
| **Phase 5: Demo Prep** | 165–180 min | Bug fixes | Final styling + responsive check | ✅ Demo ready |

---

## 👨‍💻 DEV A TASK BREAKDOWN (Data + Logic)

### 🎯 Responsibility
- Extract and flatten opportunity-level data from API response
- Implement owner-based filtering logic
- Compute category aggregations for charts
- Create reusable data helper functions
- Implement CSV export (frontend only)

### 📁 Files You Own
- **NEW**: `/app/js/dataHelpers.js` (all data functions)
- **MODIFY**: `/app/js/pipelineDashboard.js` (integrate your functions; logical updates only)

### ✅ Phase 1: Setup (0–25 min)

**Task 1A: Create `/app/js/dataHelpers.js`**

Create the file with this initial scaffold (copy-paste ready):

```javascript
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
```

**Task 1B: Update `/app/pipeline-dashboard.html`**

In the `<head>` section, add script import for dataHelpers (after authService):

```html
<script src="/app/js/dataHelpers.js"></script>
```

### ✅ Phase 2: Core Filtering & Data (25–65 min)

**Task 2A: Add Owner Filter UI Reference**

In `dataHelpers.js`, add a function to initialize the owner filter dropdown (Dev B will add HTML):

```javascript
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
```

**Task 2B: Integrate Filter Logic into pipelineDashboard.js**

At the end of the `loadWeeklyReport()` function in `/app/js/pipelineDashboard.js`, add:

```javascript
        // After renderMovementBreakdown() call, add:
        var allOpps = flattenOpportunities(data);
        initializeOwnerFilter(allOpps);
        window._currentOpportunitiesCache = allOpps; // Cache for filtering
```

Also add an event listener for the owner filter dropdown in `pipelineDashboard.js` (in the initialization code):

```javascript
document.getElementById('owner-filter-select').addEventListener('change', function() {
    var selectedOwner = this.value || null;
    var allOpps = window._currentOpportunitiesCache || [];
    var filtered = filterByOwner(allOpps, selectedOwner);
    
    // Update charts and summaries (Dev B will implement renderCharts)
    if (window.renderCharts) {
        window.renderCharts(filtered);
    }
});
```

### ✅ Phase 3: Charts & Export Data (65–120 min)

**Task 3A: Add Export Function Hook**

In `dataHelpers.js`, add:

```javascript
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
```

**Task 3B: Add Chart Data Computation Functions**

In `dataHelpers.js`, add:

```javascript
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
```

**Task 3C: Wire Data to Export Button**

Dev B will add the export button HTML. You just ensure the export function is available globally (done in 3A).

### ✅ Phase 4 & 5: Testing & Bug Fixes (120–180 min)

**Task 4A: Verify Data Flow**

Test in browser console:
```javascript
// Should return all opportunities
flattenOpportunities(window._lastApiData)

// Should return owner names
getUniqueOwners(window._currentOpportunitiesCache)

// Should return filtered opps
filterByOwner(window._currentOpportunitiesCache, 'John Doe')

// Should return aggregation
aggregateByCategory(window._currentOpportunitiesCache)

// Export should trigger download
exportCurrentData()
```

**Task 4B: Debug Filter Event**

- Test owner filter dropdown changes
- Verify chart updates when filter changes (Dev B will implement)
- Verify CSV export includes filtered data

---

## 👩‍💻 DEV B TASK BREAKDOWN (UI + Charts)

### 🎯 Responsibility
- Add chart containers with Tailwind layout
- Integrate Chart.js via CDN
- Create 6-card movement summary visual
- Implement chart rendering functions
- Polish UI for demo

### 📁 Files You Own
- **MODIFY**: `/app/pipeline-dashboard.html` (add containers, UI elements)
- **NEW**: `/app/js/chartRenderer.js` (all chart rendering logic)
- **MODIFY**: `/app/app.js` (add Chart.js CDN script)

### ✅ Phase 1: Setup (0–25 min)

**Task 1A: Add Chart.js CDN to `/app/app.js`**

Open `/app/app.js` and add this at the top of the file (before other scripts load):

```javascript
// Load Chart.js from CDN
var chartScript = document.createElement('script');
chartScript.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.js';
chartScript.async = false;
document.head.appendChild(chartScript);
```

**Task 1B: Create `/app/js/chartRenderer.js`**

Create a new file with this scaffold:

```javascript
/**
 * Chart Rendering — Handles all Chart.js initialization and updates
 */

var CHART_COLORS = {
    ce: '#0050A0',        // primary
    ms: '#00AEEF',        // primary-light
    new: '#2563EB',       // info
    won: '#16A34A',       // success
    lost: '#DC2626',      // error
    increase: '#16A34A',  // success
    decrease: '#D97706',  // warning
    removed: '#D97706'    // warning
};

var chartInstances = {};

/**
 * Initialize or update the movement bar chart.
 * @param {Array} chartData — from getChartDataMovementBars()
 */
function renderMovementBarChart(chartData) {
    var ctx = document.getElementById('chart-movement-bars');
    if (!ctx) return;
    
    // Prepare datasets
    var ceValues = chartData.map(function(d) { return d.ceTotal; });
    var msValues = chartData.map(function(d) { return d.msTotal; });
    var labels = chartData.map(function(d) { return d.category; });
    
    // Destroy existing chart if any
    if (chartInstances.movementBars) {
        chartInstances.movementBars.destroy();
    }
    
    // Create new chart
    chartInstances.movementBars = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'CE',
                    data: ceValues,
                    backgroundColor: CHART_COLORS.ce,
                    borderColor: CHART_COLORS.ce,
                    borderWidth: 1
                },
                {
                    label: 'MS',
                    data: msValues,
                    backgroundColor: CHART_COLORS.ms,
                    borderColor: CHART_COLORS.ms,
                    borderWidth: 1
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: { position: 'bottom' }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return '€' + (value / 1000).toFixed(0) + 'k';
                        }
                    }
                }
            }
        }
    });
}

/**
 * Initialize or update the type split doughnut chart.
 * @param {Object} typeData — from getChartDataTypeSplit()
 */
function renderTypeSplitChart(typeData) {
    var ctx = document.getElementById('chart-type-split');
    if (!ctx) return;
    
    // Destroy existing chart if any
    if (chartInstances.typeSplit) {
        chartInstances.typeSplit.destroy();
    }
    
    // Create new chart
    chartInstances.typeSplit = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['CE', 'MS'],
            datasets: [{
                data: [typeData.ce, typeData.ms],
                backgroundColor: [CHART_COLORS.ce, CHART_COLORS.ms],
                borderColor: ['#fff', '#fff'],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: { position: 'bottom' }
            }
        }
    });
}

/**
 * Initialize or update the waterfall chart (approximation with line).
 * @param {Array} waterfallData — from getChartDataWaterfall()
 */
function renderWaterfallChart(waterfallData) {
    var ctx = document.getElementById('chart-waterfall');
    if (!ctx) return;
    
    // Simple line chart approximation
    // For true waterfall, we'd need a custom implementation
    var labels = waterfallData.map(function(d) { return d.name; });
    var values = waterfallData.map(function(d) { return d.value; });
    
    // Destroy existing chart if any
    if (chartInstances.waterfall) {
        chartInstances.waterfall.destroy();
    }
    
    // Create line chart as waterfall approximation
    chartInstances.waterfall = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Pipeline Balance',
                data: values,
                borderColor: CHART_COLORS.ce,
                backgroundColor: 'rgba(0, 80, 160, 0.1)',
                borderWidth: 2,
                pointBackgroundColor: CHART_COLORS.ce,
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: { position: 'bottom' }
            },
            scales: {
                y: {
                    ticks: {
                        callback: function(value) {
                            return '€' + (value / 1000).toFixed(0) + 'k';
                        }
                    }
                }
            }
        }
    });
}

/**
 * Master render function — call this with filtered opportunities.
 * @param {Array} opportunities — from filterByOwner() or all opps
 * @param {Object} apiData — original API response (optional, for waterfall)
 */
window.renderCharts = function(opportunities, apiData) {
    if (!opportunities || opportunities.length === 0) {
        console.warn('No opportunities to render charts');
        return;
    }
    
    // Compute all chart data
    var movementData = getChartDataMovementBars(opportunities);
    var typeSplitData = getChartDataTypeSplit(opportunities);
    var waterfallData = getChartDataWaterfall(opportunities, apiData);
    
    // Render charts
    renderMovementBarChart(movementData);
    renderTypeSplitChart(typeSplitData);
    renderWaterfallChart(waterfallData);
};
```

**Task 1C: Add Chart.js CDN Import to HTML**

In `/app/pipeline-dashboard.html`, add to the `<head>` after the Tailwind script:

```html
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.js"></script>
```

Also add the chartRenderer script before the closing `</body>`:

```html
<script src="/app/js/chartRenderer.js"></script>
```

### ✅ Phase 2: Core UI Layout (25–65 min)

**Task 2A: Add Owner Filter Dropdown**

In `/app/pipeline-dashboard.html`, find the section with the week selector and add this AFTER the "Apply" button:

```html
<!-- Owner Filter -->
<div class="flex items-center gap-3">
  <label for="owner-filter-select" class="block text-xs font-medium text-gray-700">Filter by Owner</label>
  <select id="owner-filter-select" class="border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-700 bg-white focus:outline-none focus:ring-2 focus:ring-primary/50 focus:border-primary transition">
    <option value="">All Owners</option>
    <!-- Options populated by Dev A's initializeOwnerFilter() -->
  </select>
</div>

<!-- Export Button -->
<button id="export-csv-button" onclick="exportCurrentData()" class="inline-flex items-center justify-center bg-success text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-success-dark focus:outline-none focus:ring-2 focus:ring-success/50 focus:ring-offset-1 focus:ring-offset-gray-50 transition">
  📥 Export CSV
</button>
```

**Task 2B: Add Movement Summary Cards (6-card grid)**

In `/app/pipeline-dashboard.html`, AFTER the existing summary cards (after the Total Pipeline card), add:

```html
<!-- Movement Summary Cards (6 Categories) -->
<section>
  <h2 class="text-lg font-semibold text-gray-900 font-heading mb-4">Movement Summary</h2>
  <div class="grid grid-cols-2 md:grid-cols-3 gap-4" id="movement-summary-container">
    <!-- Cards rendered dynamically by chartRenderer.js -->
  </div>
</section>
```

Then add a function to `/app/js/chartRenderer.js` to render these cards:

```javascript
/**
 * Render the 6-card movement summary grid.
 * @param {Object} movementSummary — from computeMovementSummary()
 */
function renderMovementSummaryCards(movementSummary) {
    var container = document.getElementById('movement-summary-container');
    if (!container) return;
    
    var categoryMeta = {
        'New':      { label: 'New',      color: '#2563EB', textColor: '#1E40AF' },
        'Won':      { label: 'Won',      color: '#16A34A', textColor: '#15803D' },
        'Lost':     { label: 'Lost',     color: '#DC2626', textColor: '#991B1B' },
        'Increase': { label: 'Increase', color: '#16A34A', textColor: '#15803D' },
        'Decrease': { label: 'Decrease', color: '#D97706', textColor: '#B45309' },
        'Removed':  { label: 'Removed',  color: '#D97706', textColor: '#B45309' }
    };
    
    var html = '';
    ['New', 'Won', 'Lost', 'Increase', 'Decrease', 'Removed'].forEach(function(cat) {
        var meta = categoryMeta[cat];
        var data = movementSummary[cat];
        var percentage = data.percentage || 0;
        
        html += '<div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4">';
        html += '<div style="border-left: 4px solid ' + meta.color + ';" class="pl-3">';
        html += '<p class="text-xs font-medium text-gray-600 uppercase">' + meta.label + '</p>';
        html += '<p class="text-lg font-bold mt-1" style="color: ' + meta.textColor + ';">€' + Math.abs(data.total).toLocaleString('en-IE') + '</p>';
        html += '<p class="text-xs text-gray-500 mt-1">' + data.count + ' opps • ' + percentage + '%</p>';
        html += '</div>';
        html += '</div>';
    });
    
    container.innerHTML = html;
}
```

Update the `renderCharts()` function in chartRenderer.js to also call this:

```javascript
window.renderCharts = function(opportunities, apiData) {
    if (!opportunities || opportunities.length === 0) {
        console.warn('No opportunities to render charts');
        return;
    }
    
    // Compute all chart data
    var movementData = getChartDataMovementBars(opportunities);
    var typeSplitData = getChartDataTypeSplit(opportunities);
    var waterfallData = getChartDataWaterfall(opportunities, apiData);
    var movementSummaryData = computeMovementSummary(opportunities);
    
    // Render charts and summary
    renderMovementSummaryCards(movementSummaryData);
    renderMovementBarChart(movementData);
    renderTypeSplitChart(typeSplitData);
    renderWaterfallChart(waterfallData);
};
```

**Task 2C: Add Chart Containers**

In `/app/pipeline-dashboard.html`, add this AFTER the Movement Breakdown section and BEFORE the empty state:

```html
<!-- Charts Section -->
<section id="charts-section" class="hidden">
  <h2 class="text-lg font-semibold text-gray-900 font-heading mb-4">Analysis Charts</h2>
  
  <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
    <!-- Movement Bar Chart -->
    <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <h3 class="text-sm font-semibold text-gray-900 font-heading mb-4">Movement by Category</h3>
      <canvas id="chart-movement-bars" style="max-height: 300px;"></canvas>
    </div>
    
    <!-- Type Split Chart -->
    <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6">
      <h3 class="text-sm font-semibold text-gray-900 font-heading mb-4">Type Split (CE vs MS)</h3>
      <canvas id="chart-type-split" style="max-height: 300px;"></canvas>
    </div>
    
    <!-- Waterfall Chart -->
    <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-6 lg:col-span-2">
      <h3 class="text-sm font-semibold text-gray-900 font-heading mb-4">Pipeline Waterfall</h3>
      <canvas id="chart-waterfall" style="max-height: 300px;"></canvas>
    </div>
  </div>
</section>
```

### ✅ Phase 3: Chart Rendering & Wiring (65–120 min)

**Task 3A: Wire Chart Rendering to Data Flow**

In `/app/js/pipelineDashboard.js`, find the `loadWeeklyReport()` function and after the `renderMovementBreakdown()` call, add:

```javascript
        // Show charts section
        var chartsSection = document.getElementById('charts-section');
        if (chartsSection) chartsSection.classList.remove('hidden');
        
        // Render charts
        if (window.renderCharts) {
            window.renderCharts(allOpps, data);
        }
```

**Task 3B: Test Chart Rendering**

- Load the page and select a week
- Verify charts appear
- Check responsive layout (should stack on mobile)
- Verify colors match Tailwind theme

### ✅ Phase 4: Polish & Responsive Design (120–165 min)

**Task 4A: Responsive Refinements**

Update chart containers in HTML to ensure responsive behavior:
- Grid should be 1 col on mobile, 2 on medium, 2 on large
- Movement summary cards: 2 on mobile, 3 on medium/large
- Adjust padding and gaps for smaller screens

**Task 4B: Visual Polish**

- Check text contrast (WCAG AA)
- Add subtle shadows and borders
- Ensure consistent spacing
- Test on 375px, 768px, 1440px viewports

**Task 4C: Loading State**

Update loading indicator to show while charts are initializing:

```javascript
function setLoading(show) {
    var loader = document.getElementById('loading-indicator');
    if (loader) {
        loader.classList.toggle('hidden', !show);
    }
    // Hide charts while loading
    var chartsSection = document.getElementById('charts-section');
    if (chartsSection) {
        chartsSection.classList.toggle('hidden', show);
    }
}
```

### ✅ Phase 5: Final Demo Prep (165–180 min)

**Task 5A: Cross-Browser Check**

- Test in Chrome, Firefox, Safari (if available)
- Verify Chart.js loads and renders
- Check for console errors

**Task 5B: Final Styling Tweaks**

- Verify colors match Rapid Circle branding
- Ensure sufficient whitespace
- Check font hierarchy and sizes
- Responsive check one more time

---

## 🔗 INTEGRATION POINTS

### Checkpoint at ~90 minutes (Phase 3 start)

**Dev A delivers:**
- ✅ `dataHelpers.js` complete with all functions
- ✅ Owner filter initialization working
- ✅ Data aggregation functions tested
- ✅ CSV export function ready

**Dev B delivers:**
- ✅ HTML containers added (filter, buttons, chart canvases)
- ✅ Chart.js CDN integrated
- ✅ Chart rendering scaffold ready

### Integration Task (120 min mark)

Both devs verify:
1. ✅ Filter dropdown populates with owners
2. ✅ Changing filter triggers chart re-render
3. ✅ Export button downloads CSV with filtered data
4. ✅ Charts update when filter changes

---

## 📊 FILE OWNERSHIP MATRIX

| File | Owner | Changes |
|------|-------|---------|
| `/app/js/dataHelpers.js` | **Dev A** | CREATE |
| `/app/js/chartRenderer.js` | **Dev B** | CREATE |
| `/app/pipeline-dashboard.html` | **Dev B** | ADD containers, filter, export, charts |
| `/app/js/pipelineDashboard.js` | **Dev A** | WIRE filter, data caching |
| `/app/app.js` | **Dev B** | ADD Chart.js CDN |

**NO CONFLICTS** — Dev A and Dev B never edit the same sections of the same file.

---

## ⚠️ RISKS & FALLBACKS

| Risk | Fallback |
|------|----------|
| Chart.js CDN slow | Use local CDN or simplified SVG bars |
| Filter not populating | Hard-code sample owners for demo |
| Charts crash | Disable charts section, keep summary cards |
| CSV export fails | Copy-paste opportunity table to Excel |
| Responsive issues | Demo on single viewport size (1440px) |
| Data flow broken | Use mock data object in browser console |

---

## 🧪 TESTING CHECKLIST (Before Demo)

- [ ] Week selector loads data
- [ ] Owner filter dropdown has owners
- [ ] Changing owner updates all displays
- [ ] Bar chart renders with correct values
- [ ] Doughnut chart shows CE vs MS split
- [ ] Waterfall chart shows progression
- [ ] Summary cards show correct totals and percentages
- [ ] CSV export downloads file with filtered data
- [ ] All responsive at 375px, 768px, 1440px
- [ ] No console errors
- [ ] Browser Back button works
- [ ] Movement breakdown still expands/collapses

---

## 🎯 DEMO PRIORITY (if time runs out)

**MUST HAVE:**
1. ✅ Owner Filter (working)
2. ✅ Movement Summary Cards (6 cards visible)
3. ✅ Bar Chart (movement by category)
4. ✅ CSV Export (button works)

**NICE TO HAVE:**
5. ✅ Type Split Chart (doughnut)
6. ✅ Waterfall Chart (line approximation)

**IF TIME PERMITS:**
7. Weekly Comparison (fallback if backend data missing)
8. Advanced styling polish
9. Animation transitions

---

## 🚀 QUICK START (Copy-Paste Checklist)

### Dev A — Minute 0
- [ ] Create `/app/js/dataHelpers.js` (paste scaffold)
- [ ] Add `<script src="/app/js/dataHelpers.js"></script>` to HTML head
- [ ] Add owner filter initialization to pipelineDashboard.js
- [ ] Add export function to window

### Dev B — Minute 0
- [ ] Add Chart.js CDN to `/app/app.js`
- [ ] Add Chart.js CDN script tag to HTML
- [ ] Create `/app/js/chartRenderer.js` (paste scaffold)
- [ ] Add chart containers to HTML

### Both — Minute 30
- [ ] Test data flow
- [ ] Wire filter to charts
- [ ] Verify no console errors

### Minute 90
- [ ] Integration checkpoint
- [ ] Polish and responsive checks

### Minute 165
- [ ] Final testing
- [ ] Demo walkthrough

---

**You've got this. Keep shipping.** 🚢

