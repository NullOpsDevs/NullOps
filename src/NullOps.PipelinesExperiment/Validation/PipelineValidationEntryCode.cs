namespace NullOps.PipelinesExperiment.Validation;

public enum PipelineValidationEntryMessage
{
    StartOfPipelineIsEmpty,
    RootComponentIsEmpty,
    StepReferencesUnknownComponentId,
    SomeStepIdsAreNotUnique,
    CycleDetectedInPipeline
}