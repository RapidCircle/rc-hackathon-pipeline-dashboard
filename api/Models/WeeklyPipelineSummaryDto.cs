namespace Api.Models;

/// <summary>
/// Top-level summary of the weekly pipeline change report.
/// Contains per-type summaries and an overall total.
/// </summary>
public class WeeklyPipelineSummaryDto
{
    /// <summary>
    /// The start date of the reporting week in "yyyy-MM-dd" format.
    /// </summary>
    public string WeekStartDate { get; set; } = string.Empty;

    /// <summary>
    /// The end date of the reporting week in "yyyy-MM-dd" format.
    /// </summary>
    public string WeekEndDate { get; set; } = string.Empty;

    /// <summary>
    /// Total starting weighted value across all opportunity types.
    /// </summary>
    public double TotalStartingWeightedValue { get; set; }

    /// <summary>
    /// Total ending weighted value across all opportunity types.
    /// </summary>
    public double TotalEndingWeightedValue { get; set; }

    /// <summary>
    /// Total net change across all opportunity types.
    /// </summary>
    public double TotalNetChange { get; set; }

    /// <summary>
    /// Per-type summaries (System Integration (CE) and Managed Services).
    /// </summary>
    public List<WeeklyPipelineTypeSummaryDto> TypeSummaries { get; set; } = new();
}
