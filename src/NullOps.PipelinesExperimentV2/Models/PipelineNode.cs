namespace NullOps.PipelinesExperimentV2.Models;

public class PipelineNode
{
    public required string Name { get; set; }
    
    public required string Id { get; set; }
    
    public string? Description { get; set; }
}