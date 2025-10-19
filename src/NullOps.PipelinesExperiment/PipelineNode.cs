using NullOps.PipelinesExperiment.Components;

namespace NullOps.PipelinesExperiment;

public class PipelineNode
{
    public string StepId { get; set; }
    
    public string ComponentId { get; set; }
    
    public RegisteredPipelineComponent Component { get; set; }
    
    public PipelineNodeConfiguration[] Parameters { get; set; }
    
    public PipelineNode[] NextNodes { get; set; }
}
