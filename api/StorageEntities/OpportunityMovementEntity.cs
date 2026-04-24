using Azure;
using Azure.Data.Tables;

namespace Api.StorageEntities;

/// <summary>
/// Represents a single opportunity movement within a weekly reporting period.
/// Table: OpportunityMovements
/// PartitionKey: "{OpportunityType}_{WeekStartDate:yyyy-MM-dd}" (e.g., "SystemIntegrationCE_2026-02-09")
/// RowKey: "{MovementCategory}_{OpportunityId}" (e.g., "Won_opp-001")
/// </summary>
public class OpportunityMovementEntity : ITableEntity
{
    /// <summary>
    /// Composite key: "{OpportunityType}_{WeekStartDate:yyyy-MM-dd}".
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// Composite key: "{MovementCategory}_{OpportunityId}".
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
    /// The unique identifier of the opportunity in the source system.
    /// </summary>
    public string OpportunityId { get; set; } = string.Empty;

    /// <summary>
    /// The opportunity title/name.
    /// </summary>
    public string OpportunityTitle { get; set; } = string.Empty;

    /// <summary>
    /// The customer/account name associated with the opportunity.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// The opportunity owner's name.
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// The movement category (e.g., "New", "Won", "Lost", "Increase", "Decrease", "Removed").
    /// Stored as string representation of MovementCategory enum.
    /// </summary>
    public string MovementCategory { get; set; } = string.Empty;

    /// <summary>
    /// The opportunity type (e.g., "SystemIntegrationCE", "ManagedServices").
    /// Stored as string representation of OpportunityType enum.
    /// </summary>
    public string OpportunityType { get; set; } = string.Empty;

    /// <summary>
    /// The week start date for this movement (UTC).
    /// </summary>
    public DateTime WeekStartDate { get; set; }

    /// <summary>
    /// The weighted revenue amount associated with this movement.
    /// For new opportunities: the initial weighted revenue.
    /// For won/lost: the weighted revenue removed from open pipeline.
    /// For increases/decreases: the change amount (positive for increase, negative for decrease).
    /// </summary>
    public double WeightedRevenueChange { get; set; }

    /// <summary>
    /// The weighted revenue at the start of the week (before movement), if applicable.
    /// </summary>
    public double? PreviousWeightedRevenue { get; set; }

    /// <summary>
    /// The weighted revenue at the end of the week (after movement), if applicable.
    /// </summary>
    public double? CurrentWeightedRevenue { get; set; }

    /// <summary>
    /// The final sales stage at the end of the week or at closing.
    /// </summary>
    public string FinalSalesStage { get; set; } = string.Empty;

    /// <summary>
    /// When this record was created (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
