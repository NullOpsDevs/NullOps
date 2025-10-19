namespace NullOps.PipelinesExperiment;

public class PipelineNodeConfiguration
{
    public string StepId { get; set; }
    
    public string ComponentId { get; set; }
    
    public PipelineNodeConfigurationParameter[] Parameters { get; set; }
    
    public string[] NextComponents { get; set; }
}