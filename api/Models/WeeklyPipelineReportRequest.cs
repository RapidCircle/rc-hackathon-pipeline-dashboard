namespace Api.Models;

/// <summary>
/// Request DTO for retrieving a weekly pipeline change report.
/// The week start date identifies the reporting period.
/// </summary>
public class WeeklyPipelineReportRequest
{
    /// <summary>
    /// The start date of the reporting week (UTC).
    /// Format: "yyyy-MM-dd"
    /// </summary>
    public string WeekStartDate { get; set; } = string.Empty;
}
