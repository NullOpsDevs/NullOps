namespace NullOps.PipelinesExperimentV2.Models;

public class Component : PipelineNode
{
    /// <summary>
    /// Followers to me (next components or services)
    /// </summary>
    public List<string> FollowerIds { get; set; } = [];
    
    /// <summary>
    /// Subscribed to (previous components or services)
    /// </summary>
    public List<string> SubscriptionIds { get; set; } = [];
}