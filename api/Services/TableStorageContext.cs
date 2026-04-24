using Azure.Data.Tables;

namespace Api.Services;

/// <summary>
/// Provides typed accessors for Azure Table Storage tables used by the pipeline analysis feature.
/// Uses the STORAGE connection string (not AzureWebJobsStorage, which is reserved by SWA).
/// </summary>
public class TableStorageContext
{
    private readonly TableServiceClient _serviceClient;

    /// <summary>
    /// Table name for weekly pipeline snapshot data.
    /// </summary>
    public const string WeeklyPipelineSnapshotsTable = "WeeklyPipelineSnapshots";

    /// <summary>
    /// Table name for opportunity movement records.
    /// </summary>
    public const string OpportunityMovementsTable = "OpportunityMovements";

    /// <summary>
    /// Table name for opportunity lookup/reference data.
    /// </summary>
    public const string OpportunityLookupTable = "OpportunityLookup";

    public TableStorageContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("STORAGE")
            ?? throw new InvalidOperationException("STORAGE connection string is not configured.");
        _serviceClient = new TableServiceClient(connectionString);
    }

    /// <summary>
    /// Gets a TableClient for the WeeklyPipelineSnapshots table.
    /// </summary>
    public TableClient WeeklyPipelineSnapshots => _serviceClient.GetTableClient(WeeklyPipelineSnapshotsTable);

    /// <summary>
    /// Gets a TableClient for the OpportunityMovements table.
    /// </summary>
    public TableClient OpportunityMovements => _serviceClient.GetTableClient(OpportunityMovementsTable);

    /// <summary>
    /// Gets a TableClient for the OpportunityLookup table.
    /// </summary>
    public TableClient OpportunityLookup => _serviceClient.GetTableClient(OpportunityLookupTable);

    /// <summary>
    /// Ensures all required tables exist in storage.
    /// Call during application startup or before first use.
    /// </summary>
    public async Task EnsureTablesExistAsync()
    {
        await WeeklyPipelineSnapshots.CreateIfNotExistsAsync();
        await OpportunityMovements.CreateIfNotExistsAsync();
        await OpportunityLookup.CreateIfNotExistsAsync();
    }
}
