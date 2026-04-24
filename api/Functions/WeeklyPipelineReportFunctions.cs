using System.Globalization;
using System.Net;
using System.Text.Json;
using Api.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Api.Functions;

/// <summary>
/// HTTP-triggered Azure Functions for weekly pipeline report endpoints.
/// </summary>
public class WeeklyPipelineReportFunctions
{
    private readonly PipelineReportService _reportService;
    private readonly ILogger<WeeklyPipelineReportFunctions> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public WeeklyPipelineReportFunctions(PipelineReportService reportService, ILogger<WeeklyPipelineReportFunctions> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the weekly pipeline report for the specified week.
    /// Query parameter: weekStart=YYYY-MM-DD
    /// </summary>
    [Function("GetWeeklyReport")]
    public async Task<HttpResponseData> GetWeeklyReport(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "pipeline/weekly-report")] HttpRequestData req)
    {
        var queryParams = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var weekStartParam = queryParams["weekStart"];

        // Validate weekStart is present
        if (string.IsNullOrWhiteSpace(weekStartParam))
        {
            _logger.LogWarning("Missing weekStart query parameter");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest,
                "Missing required query parameter 'weekStart'. Expected format: YYYY-MM-DD");
        }

        // Parse and validate weekStart format
        if (!DateTime.TryParseExact(weekStartParam, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsedDate))
        {
            _logger.LogWarning("Invalid weekStart format: {WeekStart}", weekStartParam);
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest,
                "Invalid 'weekStart' format. Expected format: YYYY-MM-DD");
        }

        // Convert to UTC
        var weekStartUtc = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

        _logger.LogInformation("Fetching weekly report for weekStart={WeekStart}", weekStartParam);

        try
        {
            var report = await _reportService.GetWeeklyReportAsync(weekStartUtc);

            if (report == null)
            {
                _logger.LogInformation("No data found for weekStart={WeekStart}", weekStartParam);
                return await CreateErrorResponse(req, HttpStatusCode.NotFound,
                    $"No pipeline data found for week starting {weekStartParam}");
            }

            return await CreateJsonResponse(req, HttpStatusCode.OK, report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weekly report for weekStart={WeekStart}", weekStartParam);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "An error occurred while retrieving the weekly report");
        }
    }

    private static async Task<HttpResponseData> CreateJsonResponse<T>(HttpRequestData req, HttpStatusCode statusCode, T data)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(JsonSerializer.Serialize(data, JsonOptions));
        return response;
    }

    private static async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        return await CreateJsonResponse(req, statusCode, new { error = message });
    }
}
