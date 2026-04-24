namespace Api.Models;

/// <summary>
/// Summary of a single movement category within a weekly pipeline report.
/// Shows the total contribution of this category to the net change.
/// </summary>
public class MovementCategorySummaryDto
{
    /// <summary>
    /// The movement category (e.g., "New", "Won", "Lost", "Increase", "Decrease", "Removed").
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Display label for the category.
    /// </summary>
    public string CategoryDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Total weighted revenue change for this category.
    /// Positive for categories that increase pipeline (New, Increase).
    /// Negative for categories that decrease pipeline (Won, Lost, Decrease, Removed).
    /// </summary>
    public double TotalWeightedRevenueChange { get; set; }

    /// <summary>
    /// Number of opportunities in this category.
    /// </summary>
    public int OpportunityCount { get; set; }

    /// <summary>
    /// Individual opportunity details for this category.
    /// </summary>
    public List<OpportunityMovementDetailDto> Opportunities { get; set; } = new();
}
