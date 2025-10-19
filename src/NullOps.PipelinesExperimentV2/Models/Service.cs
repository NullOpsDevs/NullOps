namespace NullOps.PipelinesExperimentV2.Models;

public class Service : PipelineNode
{
    /// <summary>
    /// Followers to me (next components or services)
    /// </summary>
    public string[] FollowerIds { get; set; } = [];
}