using Azure;
using Azure.Data.Tables;

namespace Api.StorageEntities;

/// <summary>
/// Represents a weekly snapshot of the open pipeline for a specific opportunity type.
/// Table: WeeklyPipelineSnapshots
/// PartitionKey: OpportunityType (e.g., "SystemIntegrationCE", "ManagedServices")
/// RowKey: WeekStartDate in "yyyy-MM-dd" format
/// </summary>
public class WeeklyPipelineSnapshotEntity : ITableEntity
{
    /// <summary>
    /// The opportunity type (e.g., "SystemIntegrationCE", "ManagedServices").
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// The week start date in "yyyy-MM-dd" format.
    /// </summary>
    public string RowKey { get; set; } = string.Empty;

    /// <summary>
    /// Azure Table Storage timestamp (UTC).
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Azure Table Storage ETag for concurrency.
    /// </summary>
    public ETag ETag { get; set; }

    /// <summary>
    /// The week start date (UTC).
    /// </summary>
    public DateTime WeekStartDate { get; set; }

    /// <summary>
    /// The week end date (UTC).
    /// </summary>
    public DateTime WeekEndDate { get; set; }

    /// <summary>
    /// Total weighted revenue of open opportunities at the start of the week (UTC snapshot time).
    /// </summary>
    public double StartingWeightedValue { get; set; }

    /// <summary>
    /// Total weighted revenue of open opportunities at the end of the week (UTC snapshot time).
    /// </summary>
    public double EndingWeightedValue { get; set; }

    /// <summary>
    /// Net change in weighted revenue (EndingWeightedValue - StartingWeightedValue).
    /// </summary>
    public double NetChange { get; set; }

    /// <summary>
    /// Number of open opportunities at the start of the week.
    /// </summary>
    public int StartingOpportunityCount { get; set; }

    /// <summary>
    /// Number of open opportunities at the end of the week.
    /// </summary>
    public int EndingOpportunityCount { get; set; }

    /// <summary>
    /// When this snapshot was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// When this snapshot was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
