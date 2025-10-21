namespace NullOps.PipelinesExperimentV3.Configuration;

public class PipelineStepConfiguration
{
    public Guid StepId { get; set; }
    
    public Guid ComponentId { get; set; }
    
    public Dictionary<string, string> Parameters { get; set; }
    
    public Guid[] Dependencies { get; set; }
}
