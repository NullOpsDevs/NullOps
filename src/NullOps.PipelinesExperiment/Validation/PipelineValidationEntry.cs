namespace NullOps.PipelinesExperiment.Validation;

public class PipelineValidationEntry
{
    public required string? StepId { get; set; }
    public required PipelinePart Part { get; set; }
    public required PipelineValidationEntryMessage Message { get; set; }
    public required bool IsError { get; set; }
}