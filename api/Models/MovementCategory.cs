namespace Api.Models;

/// <summary>
/// Defines the categories of opportunity movements that explain weekly pipeline changes.
/// Each category contributes to the net change in open pipeline weighted value.
/// The string representation is used in RowKey composition for OpportunityMovementEntity.
/// </summary>
public enum MovementCategory
{
    /// <summary>
    /// New opportunities added to the open pipeline during the week.
    /// Contributes a positive change (increases open pipeline).
    /// </summary>
    New,

    /// <summary>
    /// Opportunities moved to Won status during the week.
    /// Contributes a negative change (decreases open pipeline as the deal is closed).
    /// </summary>
    Won,

    /// <summary>
    /// Opportunities moved to Lost status during the week.
    /// Contributes a negative change (decreases open pipeline).
    /// </summary>
    Lost,

    /// <summary>
    /// Existing open opportunities where Weighted Revenue increased during the week.
    /// Contributes a positive change (increases open pipeline).
    /// </summary>
    Increase,

    /// <summary>
    /// Existing open opportunities where Weighted Revenue decreased during the week.
    /// Contributes a negative change (decreases open pipeline).
    /// </summary>
    Decrease,

    /// <summary>
    /// Opportunities removed from the pipeline for reasons other than Won or Lost
    /// (e.g., status changes, cancellations).
    /// Contributes a negative change (decreases open pipeline).
    /// </summary>
    Removed
}
