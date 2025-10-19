namespace NullOps.PipelinesExperimentV2.Models;

public class Event : PipelineNode
{
    /// <summary>
    /// Followers to me (next components or services)
    /// </summary>
    public string[] FollowerIds { get; set; } = [];
}