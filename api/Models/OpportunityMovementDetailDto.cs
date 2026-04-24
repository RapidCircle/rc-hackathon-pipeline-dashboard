namespace Api.Models;

/// <summary>
/// Detail of a single opportunity's contribution to a movement category.
/// Used in the opportunity-level detail listing per movement category.
/// </summary>
public class OpportunityMovementDetailDto
{
    /// <summary>
    /// The unique identifier of the opportunity.
    /// </summary>
    public string OpportunityId { get; set; } = string.Empty;

    /// <summary>
    /// The opportunity title/name.
    /// </summary>
    public string OpportunityTitle { get; set; } = string.Empty;

    /// <summary>
    /// The customer/account name.
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// The opportunity owner's name.
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;

    /// <summary>
    /// The final sales stage at the end of the week or at closing
    /// (e.g., "Closed – Won", "Closed – Lost", or current open stage).
    /// </summary>
    public string FinalSalesStage { get; set; } = string.Empty;

    /// <summary>
    /// The weighted revenue change amount for this opportunity in this category.
    /// For new opportunities: the initial weighted revenue.
    /// For won/lost: the weighted revenue removed from open pipeline.
    /// For increases/decreases: the change amount.
    /// </summary>
    public double WeightedRevenueChange { get; set; }

    /// <summary>
    /// The weighted revenue at the start of the week, if applicable.
    /// </summary>
    public double? PreviousWeightedRevenue { get; set; }

    /// <summary>
    /// The weighted revenue at the end of the week, if applicable.
    /// </summary>
    public double? CurrentWeightedRevenue { get; set; }
}
