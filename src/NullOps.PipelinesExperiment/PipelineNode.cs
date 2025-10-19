using NullOps.PipelinesExperiment.Components;

namespace NullOps.PipelinesExperiment;

public class PipelineNode
{
    public string StepId { get; set; }

    public string ComponentId { get; set; }

    public RegisteredPipelineComponent Component { get; set; }

    public PipelineNodeConfigurationParameter[] Parameters { get; set; }

    public PipelineNode[] NextNodes { get; set; }
}
