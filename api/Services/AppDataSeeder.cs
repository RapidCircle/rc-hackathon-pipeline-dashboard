using Api.Models;
using Api.StorageEntities;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace Api.Services;

/// <summary>
/// Seeds demo pipeline data (snapshots, movements, lookups) into Azure Table Storage.
/// Provides idempotent SeedAsync and destructive ResetAsync operations.
/// </summary>
public class AppDataSeeder
{
    private readonly TableStorageContext _storage;
    private readonly ILogger<AppDataSeeder> _logger;

    /// <summary>
    /// Partition key prefix used to identify demo weeks for cleanup.
    /// </summary>
    private const string DemoWeekPrefix = "2026-02";

    public AppDataSeeder(TableStorageContext storage, ILogger<AppDataSeeder> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    /// <summary>
    /// Seeds demo pipeline data if not already present (idempotent).
    /// Populates snapshots, movements, and lookup data for two demo weeks
    /// covering both opportunity types and all movement categories.
    /// </summary>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _storage.EnsureTablesExistAsync();

        // Check if data already exists for the first demo week
        var week1Key = "2026-02-02";
        try
        {
            await _storage.WeeklyPipelineSnapshots
                .GetEntityAsync<WeeklyPipelineSnapshotEntity>(
                    nameof(OpportunityType.SystemIntegrationCE), week1Key, cancellationToken: cancellationToken);
            _logger.LogInformation("Demo pipeline data already seeded (found snapshot for {WeekKey}), skipping", week1Key);
            return;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogInformation("No existing demo data found, proceeding with seed");
        }

        var now = DateTime.UtcNow;

        // --- Demo Week 1: 2026-02-02 to 2026-02-08 ---
        await SeedWeek1Async(now, cancellationToken);

        // --- Demo Week 2: 2026-02-09 to 2026-02-15 ---
        await SeedWeek2Async(now, cancellationToken);

        // --- Opportunity Lookup entries ---
        await SeedLookupDataAsync(now, cancellationToken);

        _logger.LogInformation("Demo pipeline data seeded successfully (2 weeks, both opportunity types, all movement categories)");
    }

    /// <summary>
    /// Clears all demo pipeline data and re-seeds.
    /// Deletes entities from the three tables by querying for demo-period partition keys.
    /// </summary>
    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        await _storage.EnsureTablesExistAsync();

        _logger.LogInformation("Resetting demo pipeline data...");

        await DeleteDemoSnapshotsAsync(cancellationToken);
        await DeleteDemoMovementsAsync(cancellationToken);
        await DeleteDemoLookupsAsync(cancellationToken);

        _logger.LogInformation("Demo pipeline data cleared, re-seeding...");

        await SeedAsync(cancellationToken);
    }

    #region Week 1 — 2026-02-02 to 2026-02-08

    /// <summary>
    /// Seeds Week 1 data with baseline pipeline and movements covering
    /// New, Won, and Increase categories for SystemIntegrationCE,
    /// and New, Lost, and Decrease categories for ManagedServices.
    /// </summary>
    private async Task SeedWeek1Async(DateTime now, CancellationToken ct)
    {
        var weekStart = DateTime.SpecifyKind(new DateTime(2026, 2, 2), DateTimeKind.Utc);
        var weekEnd = DateTime.SpecifyKind(new DateTime(2026, 2, 8), DateTimeKind.Utc);
        var weekKey = "2026-02-02";

        // --- SystemIntegrationCE Week 1 ---
        // Movements: New (+120,000), Won (-85,000), Increase (+30,000) => Net = +65,000
        // Start: 800,000  End: 865,000  Count: 42 -> 43 (New +1, Won -1, Increase 0 = net +0, but new adds 1, won removes 1 => net count = +1 from new only... let's say start 42, end 43)
        // Actually: New adds 1 opp, Won removes 1 opp from open pipeline, Increase changes value not count
        // Start count: 42, New: +2, Won: -1 => End count: 43
        var ceMovementsW1 = new List<OpportunityMovementEntity>
        {
            CreateMovement("SystemIntegrationCE", weekKey, weekStart, "New", "opp-ce-101",
                "Azure Migration - Contoso Ltd", "Contoso Ltd", "Sarah Mitchell",
                "Qualify", 75000, null, 75000, now),
            CreateMovement("SystemIntegrationCE", weekKey, weekStart, "New", "opp-ce-102",
                "Power Platform Rollout - Fabrikam", "Fabrikam Inc", "James Cooper",
                "Develop", 45000, null, 45000, now),
            CreateMovement("SystemIntegrationCE", weekKey, weekStart, "Won", "opp-ce-050",
                "Teams Voice Deployment - Woodgrove", "Woodgrove Bank", "Sarah Mitchell",
                "Close", -85000, 85000, 0, now),
            CreateMovement("SystemIntegrationCE", weekKey, weekStart, "Increase", "opp-ce-030",
                "Dynamics 365 Implementation - Northwind", "Northwind Traders", "James Cooper",
                "Propose", 30000, 120000, 150000, now),
        };

        // Net change: 75000 + 45000 - 85000 + 30000 = 65000
        var ceSnapshotW1 = CreateSnapshot("SystemIntegrationCE", weekKey, weekStart, weekEnd,
            800000, 865000, 65000, 42, 43, now);

        // --- ManagedServices Week 1 ---
        // Movements: New (+60,000), Lost (-45,000), Decrease (-15,000) => Net = 0
        var msMovementsW1 = new List<OpportunityMovementEntity>
        {
            CreateMovement("ManagedServices", weekKey, weekStart, "New", "opp-ms-201",
                "Managed Security Operations - Tailspin", "Tailspin Toys", "Emily Zhang",
                "Qualify", 60000, null, 60000, now),
            CreateMovement("ManagedServices", weekKey, weekStart, "Lost", "opp-ms-150",
                "Cloud Monitoring - Adventure Works", "Adventure Works", "David Okafor",
                "Propose", -45000, 45000, 0, now),
            CreateMovement("ManagedServices", weekKey, weekStart, "Decrease", "opp-ms-120",
                "Helpdesk Outsourcing - Litware", "Litware Inc", "Emily Zhang",
                "Develop", -15000, 90000, 75000, now),
        };

        // Net change: 60000 - 45000 - 15000 = 0
        var msSnapshotW1 = CreateSnapshot("ManagedServices", weekKey, weekStart, weekEnd,
            450000, 450000, 0, 28, 28, now);

        // Insert all Week 1 data
        await InsertSnapshotAsync(ceSnapshotW1, ct);
        await InsertSnapshotAsync(msSnapshotW1, ct);
        foreach (var m in ceMovementsW1) await InsertMovementAsync(m, ct);
        foreach (var m in msMovementsW1) await InsertMovementAsync(m, ct);

        _logger.LogInformation("Seeded Week 1 ({WeekKey}): CE net +65,000 / MS net 0", weekKey);
    }

    #endregion

    #region Week 2 — 2026-02-09 to 2026-02-15

    /// <summary>
    /// Seeds Week 2 data with movements covering Removed and remaining categories,
    /// ensuring every MovementCategory is represented across the two weeks.
    /// CE covers: Decrease, Removed, New
    /// MS covers: Increase, Won, Removed
    /// </summary>
    private async Task SeedWeek2Async(DateTime now, CancellationToken ct)
    {
        var weekStart = DateTime.SpecifyKind(new DateTime(2026, 2, 9), DateTimeKind.Utc);
        var weekEnd = DateTime.SpecifyKind(new DateTime(2026, 2, 15), DateTimeKind.Utc);
        var weekKey = "2026-02-09";

        // --- SystemIntegrationCE Week 2 ---
        // Start from W1 end: 865,000, 43 opps
        // Movements: New (+55,000), Decrease (-20,000), Removed (-40,000) => Net = -5,000
        var ceMovementsW2 = new List<OpportunityMovementEntity>
        {
            CreateMovement("SystemIntegrationCE", weekKey, weekStart, "New", "opp-ce-103",
                "SharePoint Modernization - Proseware", "Proseware Inc", "James Cooper",
                "Qualify", 55000, null, 55000, now),
            CreateMovement("SystemIntegrationCE", weekKey, weekStart, "Decrease", "opp-ce-025",
                "M365 Security Suite - Consolidated Messenger", "Consolidated Messenger", "Sarah Mitchell",
                "Propose", -20000, 110000, 90000, now),
            CreateMovement("SystemIntegrationCE", weekKey, weekStart, "Removed", "opp-ce-015",
                "Legacy CRM Migration - Graphic Design Institute", "Graphic Design Institute", "James Cooper",
                "Qualify", -40000, 40000, 0, now),
        };

        // Net: 55000 - 20000 - 40000 = -5000
        // Count: 43 + 1 (new) - 1 (removed) = 43
        var ceSnapshotW2 = CreateSnapshot("SystemIntegrationCE", weekKey, weekStart, weekEnd,
            865000, 860000, -5000, 43, 43, now);

        // --- ManagedServices Week 2 ---
        // Start from W1 end: 450,000, 28 opps
        // Movements: Increase (+25,000), Won (-70,000), Removed (-30,000) => Net = -75,000
        var msMovementsW2 = new List<OpportunityMovementEntity>
        {
            CreateMovement("ManagedServices", weekKey, weekStart, "Increase", "opp-ms-110",
                "Managed Cloud Infrastructure - Wide World Importers", "Wide World Importers", "David Okafor",
                "Propose", 25000, 55000, 80000, now),
            CreateMovement("ManagedServices", weekKey, weekStart, "Won", "opp-ms-100",
                "24/7 IT Support Contract - Humongous Insurance", "Humongous Insurance", "Emily Zhang",
                "Close", -70000, 70000, 0, now),
            CreateMovement("ManagedServices", weekKey, weekStart, "Removed", "opp-ms-090",
                "Network Monitoring - Margie's Travel", "Margie's Travel", "David Okafor",
                "Qualify", -30000, 30000, 0, now),
        };

        // Net: 25000 - 70000 - 30000 = -75,000
        // Count: 28 + 0 (increase doesn't add) - 1 (won) - 1 (removed) = 26
        var msSnapshotW2 = CreateSnapshot("ManagedServices", weekKey, weekStart, weekEnd,
            450000, 375000, -75000, 28, 26, now);

        // Insert all Week 2 data
        await InsertSnapshotAsync(ceSnapshotW2, ct);
        await InsertSnapshotAsync(msSnapshotW2, ct);
        foreach (var m in ceMovementsW2) await InsertMovementAsync(m, ct);
        foreach (var m in msMovementsW2) await InsertMovementAsync(m, ct);

        _logger.LogInformation("Seeded Week 2 ({WeekKey}): CE net -5,000 / MS net -75,000", weekKey);
    }

    #endregion

    #region Lookup Data

    /// <summary>
    /// Seeds OpportunityLookupEntity entries for all demo opportunities referenced in movements.
    /// </summary>
    private async Task SeedLookupDataAsync(DateTime now, CancellationToken ct)
    {
        var lookups = new List<OpportunityLookupEntity>
        {
            // SystemIntegrationCE opportunities
            CreateLookup("SystemIntegrationCE", "opp-ce-101", "Azure Migration - Contoso Ltd",
                "Contoso Ltd", "Sarah Mitchell", "Open", "Qualify", 75000, 150000, 50, now),
            CreateLookup("SystemIntegrationCE", "opp-ce-102", "Power Platform Rollout - Fabrikam",
                "Fabrikam Inc", "James Cooper", "Open", "Develop", 45000, 90000, 50, now),
            CreateLookup("SystemIntegrationCE", "opp-ce-103", "SharePoint Modernization - Proseware",
                "Proseware Inc", "James Cooper", "Open", "Qualify", 55000, 110000, 50, now),
            CreateLookup("SystemIntegrationCE", "opp-ce-050", "Teams Voice Deployment - Woodgrove",
                "Woodgrove Bank", "Sarah Mitchell", "Won", "Close", 0, 85000, 100, now),
            CreateLookup("SystemIntegrationCE", "opp-ce-030", "Dynamics 365 Implementation - Northwind",
                "Northwind Traders", "James Cooper", "Open", "Propose", 150000, 200000, 75, now),
            CreateLookup("SystemIntegrationCE", "opp-ce-025", "M365 Security Suite - Consolidated Messenger",
                "Consolidated Messenger", "Sarah Mitchell", "Open", "Propose", 90000, 120000, 75, now),
            CreateLookup("SystemIntegrationCE", "opp-ce-015", "Legacy CRM Migration - Graphic Design Institute",
                "Graphic Design Institute", "James Cooper", "Removed", "Qualify", 0, 40000, 25, now),

            // ManagedServices opportunities
            CreateLookup("ManagedServices", "opp-ms-201", "Managed Security Operations - Tailspin",
                "Tailspin Toys", "Emily Zhang", "Open", "Qualify", 60000, 120000, 50, now),
            CreateLookup("ManagedServices", "opp-ms-150", "Cloud Monitoring - Adventure Works",
                "Adventure Works", "David Okafor", "Lost", "Propose", 0, 45000, 50, now),
            CreateLookup("ManagedServices", "opp-ms-120", "Helpdesk Outsourcing - Litware",
                "Litware Inc", "Emily Zhang", "Open", "Develop", 75000, 150000, 50, now),
            CreateLookup("ManagedServices", "opp-ms-110", "Managed Cloud Infrastructure - Wide World Importers",
                "Wide World Importers", "David Okafor", "Open", "Propose", 80000, 106667, 75, now),
            CreateLookup("ManagedServices", "opp-ms-100", "24/7 IT Support Contract - Humongous Insurance",
                "Humongous Insurance", "Emily Zhang", "Won", "Close", 0, 70000, 100, now),
            CreateLookup("ManagedServices", "opp-ms-090", "Network Monitoring - Margie's Travel",
                "Margie's Travel", "David Okafor", "Removed", "Qualify", 0, 30000, 25, now),
        };

        foreach (var lookup in lookups)
        {
            await InsertLookupAsync(lookup, ct);
        }

        _logger.LogInformation("Seeded {Count} opportunity lookup entries", lookups.Count);
    }

    #endregion

    #region Factory Methods

    private static WeeklyPipelineSnapshotEntity CreateSnapshot(
        string oppType, string weekKey, DateTime weekStart, DateTime weekEnd,
        double startValue, double endValue, double netChange,
        int startCount, int endCount, DateTime now)
    {
        return new WeeklyPipelineSnapshotEntity
        {
            PartitionKey = oppType,
            RowKey = weekKey,
            WeekStartDate = weekStart,
            WeekEndDate = weekEnd,
            StartingWeightedValue = startValue,
            EndingWeightedValue = endValue,
            NetChange = netChange,
            StartingOpportunityCount = startCount,
            EndingOpportunityCount = endCount,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
    }

    private static OpportunityMovementEntity CreateMovement(
        string oppType, string weekKey, DateTime weekStart,
        string category, string oppId, string title, string customer,
        string owner, string finalStage, double change,
        double? previousRevenue, double? currentRevenue, DateTime now)
    {
        return new OpportunityMovementEntity
        {
            PartitionKey = $"{oppType}_{weekKey}",
            RowKey = $"{category}_{oppId}",
            OpportunityId = oppId,
            OpportunityTitle = title,
            CustomerName = customer,
            OwnerName = owner,
            MovementCategory = category,
            OpportunityType = oppType,
            WeekStartDate = weekStart,
            WeightedRevenueChange = change,
            PreviousWeightedRevenue = previousRevenue,
            CurrentWeightedRevenue = currentRevenue,
            FinalSalesStage = finalStage,
            CreatedAtUtc = now
        };
    }

    private static OpportunityLookupEntity CreateLookup(
        string oppType, string oppId, string title, string customer,
        string owner, string status, string salesStage,
        double weightedRevenue, double nominalValue, double probability, DateTime now)
    {
        return new OpportunityLookupEntity
        {
            PartitionKey = oppType,
            RowKey = oppId,
            OpportunityTitle = title,
            CustomerName = customer,
            OwnerName = owner,
            Status = status,
            SalesStage = salesStage,
            WeightedRevenue = weightedRevenue,
            NominalValue = nominalValue,
            Probability = probability,
            CloseDate = DateTime.SpecifyKind(new DateTime(2026, 6, 30), DateTimeKind.Utc),
            CreatedInSourceAtUtc = DateTime.SpecifyKind(new DateTime(2025, 9, 1), DateTimeKind.Utc),
            LastModifiedInSourceAtUtc = now,
            LastSyncedAtUtc = now
        };
    }

    #endregion

    #region Insert Helpers

    private async Task InsertSnapshotAsync(WeeklyPipelineSnapshotEntity entity, CancellationToken ct)
    {
        await _storage.WeeklyPipelineSnapshots.UpsertEntityAsync(entity, TableUpdateMode.Replace, ct);
    }

    private async Task InsertMovementAsync(OpportunityMovementEntity entity, CancellationToken ct)
    {
        await _storage.OpportunityMovements.UpsertEntityAsync(entity, TableUpdateMode.Replace, ct);
    }

    private async Task InsertLookupAsync(OpportunityLookupEntity entity, CancellationToken ct)
    {
        await _storage.OpportunityLookup.UpsertEntityAsync(entity, TableUpdateMode.Replace, ct);
    }

    #endregion

    #region Delete Helpers

    private async Task DeleteDemoSnapshotsAsync(CancellationToken ct)
    {
        var demoWeeks = new[] { "2026-02-02", "2026-02-09" };
        foreach (var oppType in Enum.GetValues<OpportunityType>())
        {
            var pk = oppType.ToString();
            foreach (var weekKey in demoWeeks)
            {
                try
                {
                    await _storage.WeeklyPipelineSnapshots.DeleteEntityAsync(pk, weekKey, cancellationToken: ct);
                }
                catch (RequestFailedException ex) when (ex.Status == 404)
                {
                    // Already deleted or never existed
                }
            }
        }
    }

    private async Task DeleteDemoMovementsAsync(CancellationToken ct)
    {
        var demoWeeks = new[] { "2026-02-02", "2026-02-09" };
        foreach (var oppType in Enum.GetValues<OpportunityType>())
        {
            foreach (var weekKey in demoWeeks)
            {
                var pk = $"{oppType}_{weekKey}";
                var entities = new List<OpportunityMovementEntity>();
                await foreach (var entity in _storage.OpportunityMovements
                    .QueryAsync<OpportunityMovementEntity>(e => e.PartitionKey == pk, cancellationToken: ct))
                {
                    entities.Add(entity);
                }
                foreach (var entity in entities)
                {
                    await _storage.OpportunityMovements.DeleteEntityAsync(
                        entity.PartitionKey, entity.RowKey, cancellationToken: ct);
                }
            }
        }
    }

    private async Task DeleteDemoLookupsAsync(CancellationToken ct)
    {
        foreach (var oppType in Enum.GetValues<OpportunityType>())
        {
            var pk = oppType.ToString();
            var entities = new List<OpportunityLookupEntity>();
            await foreach (var entity in _storage.OpportunityLookup
                .QueryAsync<OpportunityLookupEntity>(e => e.PartitionKey == pk, cancellationToken: ct))
            {
                entities.Add(entity);
            }
            foreach (var entity in entities)
            {
                // Only delete demo entries (those with opp-ce- or opp-ms- prefix)
                if (entity.RowKey.StartsWith("opp-ce-") || entity.RowKey.StartsWith("opp-ms-"))
                {
                    await _storage.OpportunityLookup.DeleteEntityAsync(
                        entity.PartitionKey, entity.RowKey, cancellationToken: ct);
                }
            }
        }
    }

    #endregion
}
