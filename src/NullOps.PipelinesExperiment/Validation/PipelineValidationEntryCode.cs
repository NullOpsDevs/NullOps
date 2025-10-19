namespace NullOps.PipelinesExperiment.Validation;

public enum PipelineValidationEntryMessage
{
    StartOfPipelineIsEmpty,
    RootComponentIsEmpty,
    StepReferencesUnknownComponentId,
    CycleDetectedInPipeline
}