namespace NullOps.PipelinesExperiment.Components;

public class RegisteredPipelineComponent
{
    public required string ComponentId { get; set; }

    public required RegisteredPipelineComponentType Type { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
    
    public RegisteredPipelineComponentParameter[] Parameters { get; set; }
    
    public Func<Task> InternalHandler { get; set; }
}
