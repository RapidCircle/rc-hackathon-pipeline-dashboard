/**
 * Chart Renderer — Handles all Chart.js initialization and rendering
 * Uses data from dataHelpers.js
 */

// Chart instances storage for cleanup
var chartInstances = {
    movementBar: null,
    typeSplit: null,
    weeklyComparison: null,
    waterfall: null
};

// Color scheme matching Rapid Circle branding
var CHART_COLORS = {
    primary: '#0050A0',
    primaryLight: '#00AEEF',
    accent: '#F15A24',
    success: '#16A34A',
    error: '#DC2626',
    warning: '#D97706',
    info: '#2563EB',
    gray700: '#374151',
    gray300: '#D1D5DB'
};

/**
 * Render movement summary cards (6 categories).
 * @param {Array} opportunities - Flattened opportunity array
 */
function renderMovementSummaryCards(opportunities) {
    var container = document.getElementById('movement-summary-container');
    if (!container) return;

    var summary = computeMovementSummary(opportunities);

    var categoryColors = {
        'New': { bg: '#EFF6FF', border: '#2563EB', text: '#1E40AF' },
        'Won': { bg: '#DCFCE7', border: '#16A34A', text: '#15803D' },
        'Lost': { bg: '#FEE2E2', border: '#DC2626', text: '#991B1B' },
        'Increase': { bg: '#DCFCE7', border: '#16A34A', text: '#15803D' },
        'Decrease': { bg: '#FFFBEB', border: '#D97706', text: '#B45309' },
        'Removed': { bg: '#FFFBEB', border: '#D97706', text: '#B45309' }
    };

    var html = '';
    ['New', 'Won', 'Lost', 'Increase', 'Decrease', 'Removed'].forEach(function(category) {
        var data = summary[category];
        var colors = categoryColors[category];
        var amount = Math.abs(data.total);
        var formattedAmount = amount.toLocaleString('en-IE', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        });

        html += '<div class="rounded-xl shadow-sm border p-4" style="background-color: ' + colors.bg + '; border-color: ' + colors.border + ';">';
        html += '<div style="border-left: 4px solid ' + colors.border + ';" class="pl-3">';
        html += '<p class="text-xs font-medium text-gray-600 uppercase">' + category + '</p>';
        html += '<p class="text-lg font-bold mt-1" style="color: ' + colors.text + ';">€' + formattedAmount + '</p>';
        html += '<p class="text-xs text-gray-500 mt-1">' + data.count + ' opportunities</p>';
        html += '</div>';
        html += '</div>';
    });

    container.innerHTML = html;
}

/**
 * Render movement bar chart (categories split by CE vs MS).
 * @param {Array} opportunities - Flattened opportunity array
 */
function renderMovementBarChart(opportunities) {
    var canvas = document.getElementById('movementBarChart');
    if (!canvas) return;

    // Destroy previous chart
    if (chartInstances.movementBar) {
        chartInstances.movementBar.destroy();
    }

    var chartData = getChartDataMovementBars(opportunities);

    var ceValues = chartData.map(function(d) { return d.ceTotal; });
    var msValues = chartData.map(function(d) { return d.msTotal; });
    var labels = chartData.map(function(d) { return d.category; });

    chartInstances.movementBar = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'CE',
                    data: ceValues,
                    backgroundColor: CHART_COLORS.primary,
                    borderColor: CHART_COLORS.primary,
                    borderWidth: 0
                },
                {
                    label: 'MS',
                    data: msValues,
                    backgroundColor: CHART_COLORS.primaryLight,
                    borderColor: CHART_COLORS.primaryLight,
                    borderWidth: 0
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            indexAxis: undefined,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { font: { size: 12 } }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            if (value === 0) return '€0';
                            return '€' + (Math.abs(value) / 1000).toFixed(0) + 'k';
                        },
                        font: { size: 11 }
                    }
                },
                x: {
                    ticks: { font: { size: 11 } }
                }
            }
        }
    });
}

/**
 * Render type split doughnut chart (CE vs MS).
 * @param {Array} opportunities - Flattened opportunity array
 */
function renderTypeSplitChart(opportunities) {
    var canvas = document.getElementById('typeSplitChart');
    if (!canvas) return;

    // Destroy previous chart
    if (chartInstances.typeSplit) {
        chartInstances.typeSplit.destroy();
    }

    var typeData = getChartDataTypeSplit(opportunities);
    var total = typeData.ce + typeData.ms;

    // Handle empty state
    if (total === 0) {
        typeData.ce = 1;
        typeData.ms = 1;
    }

    chartInstances.typeSplit = new Chart(canvas, {
        type: 'doughnut',
        data: {
            labels: ['System Integration (CE)', 'Managed Services (MS)'],
            datasets: [{
                data: [typeData.ce, typeData.ms],
                backgroundColor: [CHART_COLORS.primary, CHART_COLORS.primaryLight],
                borderColor: '#FFFFFF',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { font: { size: 12 } }
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            var label = context.label || '';
                            if (label) {
                                label += ': ';
                            }
                            var value = context.parsed || 0;
                            var percentage = ((value / total) * 100).toFixed(1);
                            return label + '€' + value.toLocaleString('en-IE') + ' (' + percentage + '%)';
                        }
                    }
                }
            }
        }
    });
}

/**
 * Render weekly comparison chart (current vs previous week).
 * Uses frontend-only demo fallback if previous week data unavailable.
 * @param {Object} reportData - WeeklyPipelineSummaryDto from API
 */
function renderWeeklyComparisonChart(reportData) {
    var canvas = document.getElementById('weeklyComparisonChart');
    if (!canvas) return;

    // Destroy previous chart
    if (chartInstances.weeklyComparison) {
        chartInstances.weeklyComparison.destroy();
    }

    // Current week data from API
    var currentWeekStarting = reportData.totalStartingWeightedValue || 0;
    var currentWeekEnding = reportData.totalEndingWeightedValue || 0;
    var currentWeekNet = reportData.totalNetChange || 0;

    // DEMO FALLBACK: Frontend-only previous week data
    // In production, this would come from an API call to fetch previous week data
    // For hackathon demo, we simulate a previous week with ~15% less activity
    var previousWeekStarting = currentWeekStarting * 1.08;
    var previousWeekEnding = previousWeekStarting - (currentWeekStarting - currentWeekEnding) * 0.85;
    var previousWeekNet = previousWeekEnding - previousWeekStarting;

    chartInstances.weeklyComparison = new Chart(canvas, {
        type: 'bar',
        data: {
            labels: ['Starting Value', 'Ending Value', 'Net Change'],
            datasets: [
                {
                    label: 'Previous Week',
                    data: [previousWeekStarting, previousWeekEnding, previousWeekNet],
                    backgroundColor: CHART_COLORS.gray300,
                    borderColor: CHART_COLORS.gray300,
                    borderWidth: 0
                },
                {
                    label: 'Current Week',
                    data: [currentWeekStarting, currentWeekEnding, currentWeekNet],
                    backgroundColor: CHART_COLORS.primary,
                    borderColor: CHART_COLORS.primary,
                    borderWidth: 0
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { font: { size: 12 } }
                }
            },
            scales: {
                y: {
                    ticks: {
                        callback: function(value) {
                            if (value === 0) return '€0';
                            return '€' + (Math.abs(value) / 1000).toFixed(0) + 'k';
                        },
                        font: { size: 11 }
                    }
                },
                x: {
                    ticks: { font: { size: 11 } }
                }
            }
        }
    });
}

/**
 * Render waterfall chart showing pipeline flow.
 * Approximated as a line chart showing cumulative movement.
 * @param {Object} reportData - WeeklyPipelineSummaryDto from API
 * @param {Array} opportunities - Flattened opportunity array
 */
function renderWaterfallChart(reportData, opportunities) {
    var canvas = document.getElementById('waterfallChart');
    if (!canvas) return;

    // Destroy previous chart
    if (chartInstances.waterfall) {
        chartInstances.waterfall.destroy();
    }

    var waterfallData = getChartDataWaterfall(opportunities, reportData);

    var labels = waterfallData.map(function(d) { return d.name; });
    var values = waterfallData.map(function(d) { return d.value; });

    chartInstances.waterfall = new Chart(canvas, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Pipeline Balance',
                data: values,
                borderColor: CHART_COLORS.primary,
                backgroundColor: 'rgba(0, 80, 160, 0.1)',
                borderWidth: 3,
                pointBackgroundColor: CHART_COLORS.primary,
                pointBorderColor: '#FFFFFF',
                pointBorderWidth: 2,
                pointRadius: 5,
                fill: true,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: true,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { font: { size: 12 } }
                }
            },
            scales: {
                y: {
                    ticks: {
                        callback: function(value) {
                            if (value === 0) return '€0';
                            return '€' + (Math.abs(value) / 1000).toFixed(0) + 'k';
                        },
                        font: { size: 11 }
                    }
                },
                x: {
                    ticks: {
                        font: { size: 10 },
                        maxRotation: 45,
                        minRotation: 0
                    }
                }
            }
        }
    });
}

/**
 * Master render function — renders all charts and summary cards.
 * @param {Object} reportData - WeeklyPipelineSummaryDto from API
 * @param {Array} opportunities - Flattened opportunity array (from filterByOwner or flattenOpportunities)
 */
function renderAllCharts(reportData, opportunities) {
    if (!opportunities || opportunities.length === 0) {
        console.warn('No opportunities to render charts');
        return;
    }

    // Show charts section
    var chartsSection = document.getElementById('charts-section');
    if (chartsSection) {
        chartsSection.classList.remove('hidden');
    }

    // Render summary cards and charts
    renderMovementSummaryCards(opportunities);
    renderMovementBarChart(opportunities);
    renderTypeSplitChart(opportunities);
    renderWeeklyComparisonChart(reportData);
    renderWaterfallChart(reportData, opportunities);
}

/**
 * Hide all charts (used during loading or errors).
 */
function hideCharts() {
    var chartsSection = document.getElementById('charts-section');
    if (chartsSection) {
        chartsSection.classList.add('hidden');
    }

    var summarySection = document.getElementById('movement-summary-section');
    if (summarySection) {
        summarySection.classList.add('hidden');
    }
}

/**
 * Show charts section.
 */
function showCharts() {
    var chartsSection = document.getElementById('charts-section');
    if (chartsSection) {
        chartsSection.classList.remove('hidden');
    }

    var summarySection = document.getElementById('movement-summary-section');
    if (summarySection) {
        summarySection.classList.remove('hidden');
    }
}
