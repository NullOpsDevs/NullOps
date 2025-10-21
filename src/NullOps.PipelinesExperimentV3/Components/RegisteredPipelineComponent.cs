namespace NullOps.PipelinesExperimentV3.Components;

public class RegisteredPipelineComponent
{
    public required Guid ComponentId { get; set; }

    public required RegisteredPipelineComponentType Type { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public required bool IsSystemEvent { get; set; }
    
    public RegisteredPipelineComponentParameter[] Parameters { get; set; }
    
    public Func<Task> InternalHandler { get; set; }
}
