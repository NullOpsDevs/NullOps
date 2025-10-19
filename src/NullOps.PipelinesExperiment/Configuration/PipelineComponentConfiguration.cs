namespace NullOps.PipelinesExperiment.Configuration;

public class PipelineComponentConfiguration
{
    public string StepId { get; set; }
    
    public string ComponentId { get; set; }
    
    public PipelineComponentParameterConfiguration[] Parameters { get; set; }
    
    public string[] NextComponents { get; set; }
}
