namespace Api.Models;

/// <summary>
/// Defines the types of opportunities tracked in the pipeline.
/// In v1, only System Integration (CE) and Managed Services are in scope.
/// The string representation is used as the PartitionKey in Table Storage entities.
/// </summary>
public enum OpportunityType
{
    /// <summary>
    /// System Integration (CE) opportunities.
    /// </summary>
    SystemIntegrationCE,

    /// <summary>
    /// Managed Services opportunities.
    /// </summary>
    ManagedServices
}
