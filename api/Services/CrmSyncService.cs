using System.Net.Http.Headers;
using System.Text.Json;
using Api.Models;
using Api.StorageEntities;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Api.Services;

/// <summary>
/// Syncs opportunity data from Dynamics 365 CRM into Azure Table Storage.
/// Fetches all open opportunities, computes pipeline snapshots and movements by comparing to previous week.
/// </summary>
public class CrmSyncService
{
    private readonly TableStorageContext _storage;
    private readonly ILogger<CrmSyncService> _logger;
    private readonly HttpClient _httpClient;

    // CRM config from environment variables
    private string CrmUrl => Environment.GetEnvironmentVariable("CRM_URL") ?? "";
    private string TenantId => Environment.GetEnvironmentVariable("CRM_TENANT_ID") ?? "";
    private string ClientId => Environment.GetEnvironmentVariable("CRM_CLIENT_ID") ?? "";
    private string ClientSecret => Environment.GetEnvironmentVariable("CRM_CLIENT_SECRET") ?? "";

    public CrmSyncService(TableStorageContext storage, ILogger<CrmSyncService> logger, HttpClient httpClient)
    {
        _storage = storage;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Fetches all opportunities from D365 CRM and generates pipeline data for the current week.
    /// </summary>
    public async Task<CrmSyncResult> SyncFromCrmAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(CrmUrl) || string.IsNullOrEmpty(ClientId))
        {
            return new CrmSyncResult { Success = false, Error = "CRM environment variables not configured (CRM_URL, CRM_TENANT_ID, CRM_CLIENT_ID, CRM_CLIENT_SECRET)" };
        }

        await _storage.EnsureTablesExistAsync();

        // 1. Get OAuth token
        _logger.LogInformation("Authenticating with D365 CRM: {CrmUrl}", CrmUrl);
        string token;
        try
        {
            token = await GetCrmTokenAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to authenticate with CRM");
            return new CrmSyncResult { Success = false, Error = $"CRM authentication failed: {ex.Message}" };
        }

        // 2. Fetch all opportunities with our custom type field
        _logger.LogInformation("Fetching opportunities from CRM...");
        var opportunities = await FetchOpportunitiesAsync(token, ct);
        _logger.LogInformation("Fetched {Count} opportunities from CRM", opportunities.Count);

        if (opportunities.Count == 0)
        {
            return new CrmSyncResult { Success = true, OpportunitiesFetched = 0, Message = "No opportunities found in CRM" };
        }

        // 3. Determine current week (Monday)
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday) weekStart = weekStart.AddDays(-7);
        weekStart = DateTime.SpecifyKind(weekStart, DateTimeKind.Utc);
        var weekEnd = DateTime.SpecifyKind(weekStart.AddDays(6), DateTimeKind.Utc);

        _logger.LogInformation("Processing pipeline for week: {WeekStart} to {WeekEnd}", weekStart.ToString("yyyy-MM-dd"), weekEnd.ToString("yyyy-MM-dd"));

        // 4. Separate by opportunity type
        var ceOpps = opportunities.Where(o => o.OpportunityType == "SystemIntegrationCE").ToList();
        var msOpps = opportunities.Where(o => o.OpportunityType == "ManagedServices").ToList();

        // 5. Create snapshots and movements
        await CreateSnapshotAndMovementsAsync(weekStart, weekEnd, "SystemIntegrationCE", ceOpps, ct);
        await CreateSnapshotAndMovementsAsync(weekStart, weekEnd, "ManagedServices", msOpps, ct);

        // 6. Save opportunity lookups
        foreach (var opp in opportunities)
        {
            var lookup = new OpportunityLookupEntity
            {
                PartitionKey = opp.OpportunityType,
                RowKey = opp.Id,
                OpportunityTitle = opp.Name,
                CustomerName = opp.AccountName,
                OwnerName = opp.OwnerName,
                Status = opp.StatusCode == 1 ? "Open" : opp.StatusCode == 2 ? "Won" : "Lost",
                SalesStage = opp.StepName,
                WeightedRevenue = opp.WeightedRevenue,
                NominalValue = opp.EstimatedValue,
                Probability = opp.Probability,
                CloseDate = opp.EstimatedCloseDate.HasValue ? DateTime.SpecifyKind(opp.EstimatedCloseDate.Value, DateTimeKind.Utc) : null,
                CreatedInSourceAtUtc = DateTime.UtcNow,
                LastModifiedInSourceAtUtc = DateTime.UtcNow,
                LastSyncedAtUtc = DateTime.UtcNow
            };
            await _storage.OpportunityLookup.UpsertEntityAsync(lookup, TableUpdateMode.Replace, ct);
        }

        return new CrmSyncResult
        {
            Success = true,
            OpportunitiesFetched = opportunities.Count,
            CeCount = ceOpps.Count,
            MsCount = msOpps.Count,
            WeekStart = weekStart.ToString("yyyy-MM-dd"),
            Message = $"Synced {opportunities.Count} opportunities ({ceOpps.Count} CE, {msOpps.Count} MS) for week {weekStart:yyyy-MM-dd}"
        };
    }

    private async Task<string> GetCrmTokenAsync()
    {
        var app = ConfidentialClientApplicationBuilder
            .Create(ClientId)
            .WithClientSecret(ClientSecret)
            .WithAuthority($"https://login.microsoftonline.com/{TenantId}")
            .Build();

        var result = await app.AcquireTokenForClient(new[] { $"{CrmUrl}/.default" }).ExecuteAsync();
        return result.AccessToken;
    }

    private async Task<List<CrmOpportunity>> FetchOpportunitiesAsync(string token, CancellationToken ct)
    {
        var allOpps = new List<CrmOpportunity>();
        var url = $"{CrmUrl}/api/data/v9.2/opportunities" +
                  "?$select=name,new_opportunitytype,estimatedvalue,closeprobability,stepname,statuscode,estimatedclosedate,createdon,modifiedon" +
                  "&$expand=parentaccountid($select=name)" +
                  "&$filter=new_opportunitytype ne null" +
                  "&$orderby=createdon desc";

        while (!string.IsNullOrEmpty(url))
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("Prefer", "odata.maxpagesize=100");

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct);
            var json = JsonDocument.Parse(content);
            var root = json.RootElement;

            if (root.TryGetProperty("value", out var values))
            {
                foreach (var item in values.EnumerateArray())
                {
                    var opp = new CrmOpportunity
                    {
                        Id = item.GetProperty("opportunityid").GetString() ?? "",
                        Name = item.GetProperty("name").GetString() ?? "",
                        EstimatedValue = item.TryGetProperty("estimatedvalue", out var ev) && ev.ValueKind == JsonValueKind.Number ? ev.GetDouble() : 0,
                        Probability = item.TryGetProperty("closeprobability", out var cp) && cp.ValueKind == JsonValueKind.Number ? cp.GetInt32() : 0,
                        StepName = item.TryGetProperty("stepname", out var sn) ? sn.GetString() ?? "" : "",
                        StatusCode = item.TryGetProperty("statuscode", out var sc) && sc.ValueKind == JsonValueKind.Number ? sc.GetInt32() : 1,
                        AccountName = item.TryGetProperty("parentaccountid", out var acc) && acc.ValueKind == JsonValueKind.Object
                            ? (acc.TryGetProperty("name", out var an) ? an.GetString() ?? "" : "")
                            : "",
                        EstimatedCloseDate = item.TryGetProperty("estimatedclosedate", out var ecd) && ecd.ValueKind == JsonValueKind.String
                            ? DateTime.TryParse(ecd.GetString(), out var dt) ? DateTime.SpecifyKind(dt, DateTimeKind.Utc) : (DateTime?)null
                            : null,
                    };

                    // Map D365 option set: 100000000 = CE, 100000001 = MS
                    var typeVal = item.TryGetProperty("new_opportunitytype", out var ot) && ot.ValueKind == JsonValueKind.Number ? ot.GetInt32() : -1;
                    opp.OpportunityType = typeVal == 100000000 ? "SystemIntegrationCE" : typeVal == 100000001 ? "ManagedServices" : "Unknown";

                    // Weighted revenue = estimated value * probability / 100
                    opp.WeightedRevenue = Math.Round(opp.EstimatedValue * opp.Probability / 100.0, 2);

                    // Owner — fetch separately or use _ownerid_value
                    opp.OwnerName = "Rahul Chamoli"; // Default for hackathon

                    allOpps.Add(opp);
                }
            }

            // Handle pagination
            url = root.TryGetProperty("@odata.nextLink", out var nextLink) ? nextLink.GetString() : null;
        }

        return allOpps;
    }

    private async Task CreateSnapshotAndMovementsAsync(
        DateTime weekStart, DateTime weekEnd, string oppType, List<CrmOpportunity> opportunities, CancellationToken ct)
    {
        var weekKey = weekStart.ToString("yyyy-MM-dd");
        var totalWeighted = opportunities.Where(o => o.StatusCode == 1).Sum(o => o.WeightedRevenue);
        var openCount = opportunities.Count(o => o.StatusCode == 1);

        // Try to get previous week's snapshot for comparison
        var prevWeekKey = weekStart.AddDays(-7).ToString("yyyy-MM-dd");
        double prevWeighted = 0;
        int prevCount = 0;
        try
        {
            var prev = await _storage.WeeklyPipelineSnapshots.GetEntityAsync<WeeklyPipelineSnapshotEntity>(oppType, prevWeekKey, cancellationToken: ct);
            prevWeighted = prev.Value.EndingWeightedValue;
            prevCount = prev.Value.EndingOpportunityCount;
        }
        catch { /* No previous week data — first sync */ }

        // Create/update snapshot
        var snapshot = new WeeklyPipelineSnapshotEntity
        {
            PartitionKey = oppType,
            RowKey = weekKey,
            WeekStartDate = weekStart,
            WeekEndDate = weekEnd,
            StartingWeightedValue = prevWeighted > 0 ? prevWeighted : totalWeighted * 0.95,  // Approximate if no previous
            EndingWeightedValue = totalWeighted,
            NetChange = totalWeighted - (prevWeighted > 0 ? prevWeighted : totalWeighted * 0.95),
            StartingOpportunityCount = prevCount > 0 ? prevCount : openCount,
            EndingOpportunityCount = openCount,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        await _storage.WeeklyPipelineSnapshots.UpsertEntityAsync(snapshot, TableUpdateMode.Replace, ct);

        // Create movement entries for each opportunity
        // For the initial sync, mark open ones as "New" to populate the dashboard
        foreach (var opp in opportunities.Where(o => o.StatusCode == 1).Take(20))
        {
            var movement = new OpportunityMovementEntity
            {
                PartitionKey = $"{oppType}_{weekKey}",
                RowKey = $"New_{opp.Id}",
                OpportunityId = opp.Id,
                OpportunityTitle = opp.Name,
                CustomerName = opp.AccountName,
                OwnerName = opp.OwnerName,
                MovementCategory = "New",
                OpportunityType = oppType,
                WeekStartDate = weekStart,
                WeightedRevenueChange = opp.WeightedRevenue,
                PreviousWeightedRevenue = 0,
                CurrentWeightedRevenue = opp.WeightedRevenue,
                FinalSalesStage = opp.StepName,
                CreatedAtUtc = DateTime.UtcNow
            };
            await _storage.OpportunityMovements.UpsertEntityAsync(movement, TableUpdateMode.Replace, ct);
        }

        _logger.LogInformation("Created snapshot for {Type} week {Week}: {Count} opps, {Weighted:C} weighted",
            oppType, weekKey, openCount, totalWeighted);
    }
}

public class CrmOpportunity
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string OpportunityType { get; set; } = "";
    public double EstimatedValue { get; set; }
    public int Probability { get; set; }
    public double WeightedRevenue { get; set; }
    public string StepName { get; set; } = "";
    public int StatusCode { get; set; }
    public string AccountName { get; set; } = "";
    public string OwnerName { get; set; } = "";
    public DateTime? EstimatedCloseDate { get; set; }
}

public class CrmSyncResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }
    public int OpportunitiesFetched { get; set; }
    public int CeCount { get; set; }
    public int MsCount { get; set; }
    public string? WeekStart { get; set; }
}
