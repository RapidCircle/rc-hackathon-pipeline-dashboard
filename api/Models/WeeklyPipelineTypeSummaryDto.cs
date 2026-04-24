namespace Api.Models;

/// <summary>
/// Summary of weekly pipeline changes for a single opportunity type.
/// </summary>
public class WeeklyPipelineTypeSummaryDto
{
    /// <summary>
    /// The opportunity type (e.g., "SystemIntegrationCE", "ManagedServices").
    /// </summary>
    public string OpportunityType { get; set; } = string.Empty;

    /// <summary>
    /// Display label for the opportunity type (e.g., "System Integration (CE)", "Managed Services").
    /// </summary>
    public string OpportunityTypeDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Total weighted revenue of open opportunities at the start of the week.
    /// </summary>
    public double StartingWeightedValue { get; set; }

    /// <summary>
    /// Total weighted revenue of open opportunities at the end of the week.
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
    /// Breakdown of movements by category for this opportunity type.
    /// </summary>
    public List<MovementCategorySummaryDto> MovementCategories { get; set; } = new();
}
