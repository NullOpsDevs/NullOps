namespace NullOps.PipelinesExperimentV3;

public class PipelineStep
{
    public Guid StepId { get; set; }
    
    public Guid ComponentId { get; set; }
    
    public Dictionary<string, string> Parameters { get; set; }
    
    public Guid[] Dependencies { get; set; }
}
