using Azure;
using Azure.Data.Tables;

namespace Api.StorageEntities;

/// <summary>
/// Stores the latest known state of each opportunity for quick lookup and comparison.
/// Table: OpportunityLookup
/// PartitionKey: OpportunityType (e.g., "SystemIntegrationCE", "ManagedServices")
/// RowKey: OpportunityId (the unique identifier from the source system)
/// </summary>
public class OpportunityLookupEntity : ITableEntity
{
    /// <summary>
    /// The opportunity type (e.g., "SystemIntegrationCE", "ManagedServices").
    /// </summary>
    public string PartitionKey { get; set; } = string.Empty;

    /// <summary>
    /// The unique identifier of the opportunity in the source system.
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
    /// The current status of the opportunity (e.g., "Open", "Won", "Lost").
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The current sales stage of the opportunity.
    /// </summary>
    public string SalesStage { get; set; } = string.Empty;

    /// <summary>
    /// The current weighted revenue (Weighted Revenue field from Dynamics CRM).
    /// </summary>
    public double WeightedRevenue { get; set; }

    /// <summary>
    /// The nominal (non-weighted) value of the opportunity.
    /// </summary>
    public double NominalValue { get; set; }

    /// <summary>
    /// The probability percentage used for weighting.
    /// </summary>
    public double Probability { get; set; }

    /// <summary>
    /// The expected close date (UTC).
    /// </summary>
    public DateTime? CloseDate { get; set; }

    /// <summary>
    /// When this opportunity was created in the source system (UTC).
    /// </summary>
    public DateTime CreatedInSourceAtUtc { get; set; }

    /// <summary>
    /// When this opportunity was last modified in the source system (UTC).
    /// </summary>
    public DateTime LastModifiedInSourceAtUtc { get; set; }

    /// <summary>
    /// When this lookup record was last synced/updated (UTC).
    /// </summary>
    public DateTime LastSyncedAtUtc { get; set; }
}
