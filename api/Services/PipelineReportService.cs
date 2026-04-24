using Api.Models;
using Api.StorageEntities;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace Api.Services;

/// <summary>
/// Core service that reads snapshot and movement entities from Table Storage
/// and produces the weekly pipeline report DTO, including reconciliation of
/// movement categories to net change.
/// </summary>
public class PipelineReportService
{
    private readonly TableStorageContext _storage;
    private readonly ILogger<PipelineReportService> _logger;

    public PipelineReportService(TableStorageContext storage, ILogger<PipelineReportService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the weekly pipeline report for the specified week.
    /// Returns null if no snapshot data exists for the given week.
    /// </summary>
    /// <param name="weekStartUtc">The Monday (start) of the reporting week, in UTC.</param>
    /// <returns>A populated WeeklyPipelineSummaryDto, or null if no data found.</returns>
    public async Task<WeeklyPipelineSummaryDto?> GetWeeklyReportAsync(DateTime weekStartUtc)
    {
        var weekKey = weekStartUtc.ToString("yyyy-MM-dd");
        _logger.LogInformation("Building weekly pipeline report for week starting {WeekKey}", weekKey);

        // 1. Query WeeklyPipelineSnapshotEntity for each opportunity type
        var snapshots = await GetSnapshotsForWeekAsync(weekKey);

        if (snapshots.Count == 0)
        {
            _logger.LogWarning("No snapshot data found for week {WeekKey}", weekKey);
            return null;
        }

        // 2. Build per-type summaries
        var typeSummaries = new List<WeeklyPipelineTypeSummaryDto>();

        foreach (var snapshot in snapshots)
        {
            var oppType = snapshot.PartitionKey; // e.g., "SystemIntegrationCE"

            // 3. Query movements for this type and week
            var movements = await GetMovementsForTypeAndWeekAsync(oppType, weekKey);

            // 4. Build movement category summaries with opportunity details
            var categorySummaries = BuildCategorySummaries(movements);

            typeSummaries.Add(new WeeklyPipelineTypeSummaryDto
            {
                OpportunityType = oppType,
                OpportunityTypeDisplayName = GetDisplayName(oppType),
                StartingWeightedValue = snapshot.StartingWeightedValue,
                EndingWeightedValue = snapshot.EndingWeightedValue,
                NetChange = snapshot.NetChange,
                StartingOpportunityCount = snapshot.StartingOpportunityCount,
                EndingOpportunityCount = snapshot.EndingOpportunityCount,
                MovementCategories = categorySummaries
            });
        }

        // 5. Compute totals across all types
        var weekEndDate = snapshots[0].WeekEndDate;
        var report = new WeeklyPipelineSummaryDto
        {
            WeekStartDate = weekKey,
            WeekEndDate = weekEndDate.ToString("yyyy-MM-dd"),
            TotalStartingWeightedValue = typeSummaries.Sum(t => t.StartingWeightedValue),
            TotalEndingWeightedValue = typeSummaries.Sum(t => t.EndingWeightedValue),
            TotalNetChange = typeSummaries.Sum(t => t.NetChange),
            TypeSummaries = typeSummaries
        };

        _logger.LogInformation(
            "Weekly report built for {WeekKey}: {TypeCount} types, total net change {NetChange:C0}",
            weekKey, typeSummaries.Count, report.TotalNetChange);

        return report;
    }

    /// <summary>
    /// Queries all WeeklyPipelineSnapshotEntity records for the given week.
    /// Each opportunity type has its own snapshot row (PartitionKey = type, RowKey = weekKey).
    /// </summary>
    private async Task<List<WeeklyPipelineSnapshotEntity>> GetSnapshotsForWeekAsync(string weekKey)
    {
        var snapshots = new List<WeeklyPipelineSnapshotEntity>();

        // Query each known opportunity type
        foreach (var oppType in Enum.GetValues<OpportunityType>())
        {
            var partitionKey = oppType.ToString();
            try
            {
                var response = await _storage.WeeklyPipelineSnapshots
                    .GetEntityAsync<WeeklyPipelineSnapshotEntity>(partitionKey, weekKey);
                snapshots.Add(response.Value);
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogDebug("No snapshot found for {Type} week {Week}", partitionKey, weekKey);
            }
        }

        return snapshots;
    }

    /// <summary>
    /// Queries all OpportunityMovementEntity records for a given opportunity type and week.
    /// PartitionKey format: "{OpportunityType}_{WeekStartDate}"
    /// </summary>
    private async Task<List<OpportunityMovementEntity>> GetMovementsForTypeAndWeekAsync(
        string opportunityType, string weekKey)
    {
        var partitionKey = $"{opportunityType}_{weekKey}";
        var movements = new List<OpportunityMovementEntity>();

        await foreach (var entity in _storage.OpportunityMovements
            .QueryAsync<OpportunityMovementEntity>(e => e.PartitionKey == partitionKey))
        {
            movements.Add(entity);
        }

        _logger.LogDebug("Found {Count} movements for {Partition}", movements.Count, partitionKey);
        return movements;
    }

    /// <summary>
    /// Groups movements by category and builds per-category summaries with opportunity details.
    /// WeightedRevenueChange values are expected to be pre-signed at data entry time
    /// (positive for New/Increase, negative for Won/Lost/Decrease/Removed).
    /// This method sums values as-is without re-applying signs.
    /// </summary>
    private static List<MovementCategorySummaryDto> BuildCategorySummaries(
        List<OpportunityMovementEntity> movements)
    {
        var grouped = movements.GroupBy(m => m.MovementCategory);

        var summaries = new List<MovementCategorySummaryDto>();

        foreach (var group in grouped)
        {
            var categoryName = group.Key;

            var opportunities = group.Select(m => new OpportunityMovementDetailDto
            {
                OpportunityId = m.OpportunityId,
                OpportunityTitle = m.OpportunityTitle,
                CustomerName = m.CustomerName,
                OwnerName = m.OwnerName,
                FinalSalesStage = m.FinalSalesStage,
                WeightedRevenueChange = m.WeightedRevenueChange,
                PreviousWeightedRevenue = m.PreviousWeightedRevenue,
                CurrentWeightedRevenue = m.CurrentWeightedRevenue
            }).ToList();

            summaries.Add(new MovementCategorySummaryDto
            {
                Category = categoryName,
                CategoryDisplayName = GetCategoryDisplayName(categoryName),
                TotalWeightedRevenueChange = opportunities.Sum(o => o.WeightedRevenueChange),
                OpportunityCount = opportunities.Count,
                Opportunities = opportunities
            });
        }

        // Ensure all categories are represented, even if empty
        foreach (var category in Enum.GetValues<MovementCategory>())
        {
            var categoryName = category.ToString();
            if (!summaries.Any(s => s.Category == categoryName))
            {
                summaries.Add(new MovementCategorySummaryDto
                {
                    Category = categoryName,
                    CategoryDisplayName = GetCategoryDisplayName(categoryName),
                    TotalWeightedRevenueChange = 0,
                    OpportunityCount = 0,
                    Opportunities = new List<OpportunityMovementDetailDto>()
                });
            }
        }

        // Sort by defined enum order
        var categoryOrder = Enum.GetValues<MovementCategory>()
            .Select((c, i) => (Name: c.ToString(), Index: i))
            .ToDictionary(x => x.Name, x => x.Index);

        summaries.Sort((a, b) =>
        {
            var aIdx = categoryOrder.GetValueOrDefault(a.Category, int.MaxValue);
            var bIdx = categoryOrder.GetValueOrDefault(b.Category, int.MaxValue);
            return aIdx.CompareTo(bIdx);
        });

        return summaries;
    }

    /// <summary>
    /// Returns a display-friendly name for an opportunity type.
    /// </summary>
    private static string GetDisplayName(string opportunityType) => opportunityType switch
    {
        nameof(OpportunityType.SystemIntegrationCE) => "System Integration (CE)",
        nameof(OpportunityType.ManagedServices) => "Managed Services",
        _ => opportunityType
    };

    /// <summary>
    /// Returns a display-friendly name for a movement category.
    /// </summary>
    private static string GetCategoryDisplayName(string category) => category switch
    {
        nameof(MovementCategory.New) => "New",
        nameof(MovementCategory.Won) => "Won",
        nameof(MovementCategory.Lost) => "Lost",
        nameof(MovementCategory.Increase) => "Increase",
        nameof(MovementCategory.Decrease) => "Decrease",
        nameof(MovementCategory.Removed) => "Removed",
        _ => category
    };
}
