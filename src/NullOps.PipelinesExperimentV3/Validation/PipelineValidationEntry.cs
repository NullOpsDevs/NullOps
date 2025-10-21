namespace NullOps.PipelinesExperimentV3.Validation;

public class PipelineValidationEntry
{
    public required Guid? StepId { get; set; }
    public required PipelineValidationEntryMessage Message { get; set; }
    public required bool IsError { get; set; }
}